using CommonData.Services;
using CommonData.Session;
using CommonData.VO;
using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommonData.DAO;

internal class RepositoryBase
{ // Thread-safe session storage using AsyncLocal
    private static readonly AsyncLocal<ISession> _session = new AsyncLocal<ISession>();

    // Lock for session initialization
    private static readonly SemaphoreSlim _sessionInitLock = new SemaphoreSlim(1 , 1);

    // Class-level fields
    private static IStatelessSession _statelessSession;
    private bool _transactionWasCommitted;
    //private EntityBase _lastEntityWork;
    //private UserTrackerVO _userTracker;
    // Default command timeout in seconds
    public static int _command_timeout = 90;
    // Default batch size for bulk operations
    private const int DEFAULT_BATCH_SIZE = 50;

    /// <summary>
    /// Gets or sets the current NHibernate session
    /// </summary>
    public static ISession Session
    {
        get => GetSession();
        set => _session.Value = value;
    }

    // Cached interceptor instance - AuditableListener is stateless so we can reuse it
    //private static readonly AuditableListener _sharedInterceptor = new AuditableListener();

    /// <summary>
    /// Gets the current session or creates a new one if needed.
    /// Improved for WinForms and async/threaded scenarios.
    /// </summary>
    public static ISession GetSession()
    {
        // Cache factory reference to avoid repeated static field access
        var sessionFactory = SessionFactoryGenerator.SessionFactory;
        if (sessionFactory is null || sessionFactory.IsClosed)
            return null;

        // Fast path: reuse existing open and connected session
        var existingSession = _session.Value;
        if (existingSession is { IsOpen: true, IsConnected: true })
            return existingSession;

        // Session exists but disconnected - try to reconnect
        if (existingSession is { IsOpen: true })
        {
            try
            {
                existingSession.Reconnect();
                return existingSession;
            }
            catch (HibernateException ex)
            {
                // Log reconnect failure for diagnostics
                CurrentLogger.Instance.Error($"Session reconnect failed, creating new session: {ex.Message}" , ex);
                existingSession.Dispose();
                _session.Value = null;
            }
        }

        // Create new session using cached interceptor
        _session.Value = sessionFactory.WithOptions()
        //.Interceptor(_sharedInterceptor)
        .OpenSession();

        return _session.Value;
    }

    /// <summary>
    /// Gets a dedicated session for an async operation
    /// </summary>
    /*public static async Task<ISession> GetSessionForAsyncOperation(CancellationToken cancellationToken = default)
    {
        // Cache factory reference
        var sessionFactory = SessionFactoryGenerator.SessionFactory;
        if (sessionFactory is null || sessionFactory.IsClosed)
            return null;

        // Fast path: reuse existing open session without locking
        var existingSession = _session.Value;
        if (existingSession is { IsOpen: true, IsConnected: true })
            return existingSession;

        // Ensure we don't create multiple sessions simultaneously
        await _sessionInitLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // Double-check after acquiring lock
            existingSession = _session.Value;
            if (existingSession is { IsOpen: true, IsConnected: true })
                return existingSession;

            // Try reconnect if session exists but disconnected
            if (existingSession is { IsOpen: true })
            {
                try
                {
                    existingSession.Reconnect();
                    return existingSession;
                }
                catch (HibernateException ex)
                {
                    CurrentLogger.Instance.Error($"Async session reconnect failed: {ex.Message}" , ex);
                    existingSession.Dispose();
                    _session.Value = null;
                }
            }

            // Create new session using cached interceptor
            _session.Value = sessionFactory.WithOptions()
            .Interceptor(_sharedInterceptor)
            .OpenSession();

            return _session.Value;
        }
        finally
        {
            _sessionInitLock.Release();
        }
    }
    /// <summary>
    /// Gets a stateless session for bulk operations
    /// </summary>
    public static IStatelessSession StatelessSession
    {
        get
        {
            if (_statelessSession == null || !_statelessSession.IsOpen)
            {
                _statelessSession = SessionFactoryGenerator.SessionFactory.OpenStatelessSession();
            }
            return _statelessSession;
        }
    }

    /// <summary>
    /// Disconnects the session and cleans up resources
    /// </summary>
    public static async Task SessionDisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (Session != null)
            {
                await new ObjectStateDAO().ReleaseAllObjectForCurrentUser().ConfigureAwait(false);
            }

            // Reset user state
            AppBaseEntity.Instance.UserCurrentStatic = null;
            Connection.CurrentConnection = null;

            if (SessionFactoryGenerator.SessionFactory != null && !SessionFactoryGenerator.SessionFactory.IsClosed)
            {
                // Clear statistics
                SessionFactoryGenerator.SessionFactory.Statistics.Clear();

                // Dispose connections properly
                using (var tempSession = SessionFactoryGenerator.SessionFactory.OpenSession())
                {
                    if (tempSession.Connection != null)
                    {
                        tempSession.Disconnect();
                    }
                }

                // Close and dispose the session factory
                SessionFactoryGenerator.SessionFactory.Close();
                SessionFactoryGenerator.SessionFactory.Dispose();
            }

            // Clean up references
            SessionFactoryGenerator.SessionFactory = null;
            _session.Value = null;

            if (_statelessSession != null && _statelessSession.IsOpen)
            {
                _statelessSession.Dispose();
                _statelessSession = null;
            }

            SessionFactoryGenerator.sqlConfiguration = null;
            SessionFactoryGenerator.garbageCollector();
        }
        catch (Exception ex)
        {
            CurrentLogger.Instance.Error($"Error during session disconnect: {ex.Message}" , ex);
        }
    }

    /// <summary>
    /// Refreshes an entity from the database
    /// </summary>
    public static async Task RefreshEntityAsync(EntityBase entity , CancellationToken cancellationToken = default)
    {
        if (entity == null || Session == null)
            return;

        await Session.RefreshAsync(entity , LockMode.None , cancellationToken).ConfigureAwait(false);
    }
    // Generic method to execute operations in a transaction
    private async Task<T> ExecuteInTransactionAsync<T>(
    Func<ISession , CancellationToken , Task<T>> operation ,
    CancellationToken cancellationToken = default ,
    IsolationLevel isolationLevel = IsolationLevel.ReadCommitted ,
    int timeoutSeconds = 30) // Add timeout parameter
    {
        using var session = await GetSessionForAsyncOperation(cancellationToken).ConfigureAwait(false);

        using var transaction = session.BeginTransaction(isolationLevel);
        try
        {
            // Create a linked token source that will also timeout if needed
            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken , timeoutSource.Token);

            var result = await operation(session , linkedSource.Token).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (OperationCanceledException ex)
        {
            await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
                throw; // User-initiated cancellation

            // If we reach here, it was a timeout
            throw new TimeoutException("The database operation timed out." , ex);
        }
        catch (Exception ex)
        {
            try
            {
                // Use a separate try-catch for rollback to ensure it doesn't mask the original exception
                await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception rollbackEx)
            {
                // Log rollback error but don't throw it
                Console.WriteLine($"Error during transaction rollback: {rollbackEx.Message}");
            }

            // Return a clean exception rather than throwing a RepositoryException
            // This makes it easier to handle specific exception types in the calling code
            if (ex is TimeoutException)
                throw; // Preserve timeout exceptions

            throw new Exception($"Transaction failed: {ex.Message}" , ex);
        }
    }
    /// <summary>
    /// Deletes an entity asynchronously
    /// </summary>
    public async Task<string> DeleteAsync(EntityBase entity , CancellationToken cancellationToken = default)
    {
        if (entity == null)
            return "Entity cannot be null";

        var msg = new StringBuilder();
        bool transactionWasCommitted = false;

        try
        {
            // Store entity information before clearing session
            var id = entity.GetType().GetProperty("Id")?.GetValue(entity);
            string entityName = ((AppBaseEntity)entity).Name;

            SessionClear();

            // Use a dedicated session for this operation
            using (var session = await GetSessionForAsyncOperation(cancellationToken).ConfigureAwait(false))
            using (var transaction = session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    // Re-load the entity with the current session
                    var freshEntity = (EntityBase)await session.GetAsync(entity.GetType() , id , cancellationToken).ConfigureAwait(false);

                    if (freshEntity == null)
                        return $"Entity {entityName} with ID {id} not found.";

                    // Delete and commit
                    await session.DeleteAsync(freshEntity , cancellationToken).ConfigureAwait(false);
                    await session.FlushAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    transactionWasCommitted = true;

                    // Track the deletion
                    var userTracker = new UserTrackerVO { TransationType = ReminderVO.TriggerKinds.Delete };
                    await endEditUserTracker(userTracker , freshEntity).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    throw;
                }
            }

            msg.Append($"{entityName} was deleted successfully");
        }
        catch (Exception ex) when (ex.InnerException?.Message?.Contains("conflicted") == true)
        {
            // Handle reference constraint violations
            string innerMessage = ExtractReferenceMessage(ex);

            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name} لا يمكن الحذف للانه يوجد له حركات \n{innerMessage}"
            : $"{((AppBaseEntity)entity).Name} was NOT deleted successfully. It has references in: \n{innerMessage}");

            if (!ex.InnerException.Message.Contains("REFERENCE"))
                await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Handle general errors
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name}\n للاسف!! لم تتم عملية الحذف \n{ex.Message}"
            : $"{((AppBaseEntity)entity).Name} could not be deleted: \n{ex.Message}");

            await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
            throw new Exception(msg.ToString() , ex);
        }

        if (!transactionWasCommitted)
            throw new Exception(msg.ToString());

        return msg.ToString();
    }

    // You'll need to add this custom exception type
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message , Exception innerException) : base(message , innerException) { }
    }
    /// <summary>
    /// Deletes a list of entities in batches
    /// </summary>
    public async Task<string> DeleteListAsync(object entity , int batchSize = 50 , CancellationToken cancellationToken = default)
    {
        // Cast to appropriate collection type
        IList collection = (IList)entity;

        // Early validation
        if (collection.Count == 0)
            return "No entities to delete";

        SessionClear();
        var resultMessage = new StringBuilder();

        try
        {
            // Process deletion with batching inside a transaction
            await ExecuteInTransactionAsync<string>(
            async (session , ct) =>
            {
                int totalProcessed = 0;
                string entityName = collection[0].GetType().Name.Replace("Proxy" , "");
                for (int batchStart = 0 ; batchStart < collection.Count ; batchStart += batchSize)
                {
                    ct.ThrowIfCancellationRequested();
                    int currentBatchSize = Math.Min(batchSize , collection.Count - batchStart);
                    for (int i = 0 ; i < currentBatchSize ; i++)
                    {
                        int index = batchStart + i;
                        EntityBase currentEntity = (EntityBase)collection[index];
                        // Load the entity from the session to ensure it is attached
                        var loadedEntity = await session.GetAsync(currentEntity.GetType() , currentEntity.Id , ct).ConfigureAwait(false);
                        if (loadedEntity != null)
                        {
                            await session.DeleteAsync(loadedEntity , ct).ConfigureAwait(false);
                            // Track the deletion
                            var userTracker = new UserTrackerVO { TransationType = ReminderVO.TriggerKinds.Delete };
                            await endEditUserTracker(userTracker , currentEntity).ConfigureAwait(false);
                            totalProcessed++;
                        }
                    }
                    // Flush and clear session after each batch
                    await session.FlushAsync(ct).ConfigureAwait(false);
                    session.Clear();
                }
                return $"{totalProcessed} items were deleted successfully";
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);
            resultMessage.Append($"{collection.Count} items were deleted successfully");
        }
        catch (OperationCanceledException)
        {
            resultMessage.Append("Operation was cancelled by user");
        }
        catch (Exception ex)
        {
            EntityBase lastEntity = collection.Count > 0 ? (EntityBase)collection[collection.Count - 1] : null;
            if (lastEntity != null)
            {
                await loggErrorMessage(lastEntity , "Batch deletion failed" , ex).ConfigureAwait(false);
            }
            resultMessage.Clear();
            resultMessage.Append($"Failed to delete {collection.Count} items: {ex.Message}");
            throw new Exception(resultMessage.ToString() , ex);
        }
        return resultMessage.ToString();
    }

    /// <summary>
    /// Merges an entity with the current session
    /// </summary>
    public async Task<string> MergeAsync(EntityBase entity , CancellationToken cancellationToken = default)
    {
        if (entity == null)
            return "Entity cannot be null";

        var msg = new StringBuilder();
        var userTracker = new UserTrackerVO();

        try
        {
            // Determine if this is an update or insert
            userTracker.TransationType = entity.Id != Guid.Empty
            ? ReminderVO.TriggerKinds.Edit
            : ReminderVO.TriggerKinds.Add;

            // Set user information
            if ((entity is InvoiceVO invoice && invoice.InvoiceProperty.IsPointOfSale && invoice.InvoiceProperty.IsTouchScreen)
                || (entity is OrderVO order && order.OrderProperty.IsTouchScreen))
            {
                if (entity.UserEdited is null)
                    entity.UserEdited = AppBaseEntity.Instance.UserCurrentStatic;
            }
            else
                entity.UserEdited = AppBaseEntity.Instance.UserCurrentStatic;

            // Clear the current session
            SessionClear();
            await ExecuteInTransactionAsync<EntityBase>
            (
            async (session , ct) =>
            {
                // Perform the merge and flush operations
                var mergedEntity = await session.MergeAsync(entity , ct).ConfigureAwait(false);
                await session.FlushAsync(ct).ConfigureAwait(false);

                return mergedEntity;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted ,
            timeoutSeconds: _command_timeout
            );


            // Handle tracking and notifications after saving
            await afterSavingAsync(userTracker , entity).ConfigureAwait(false);

            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name}\n تم الحفظ بنجاح"
            : $"{((AppBaseEntity)entity).Name} was saved successfully");
        }
        catch (TaskCanceledException ex)
        {
            msg.Append("The operation was canceled. Please try again.");
            await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
            throw new OperationCanceledException(msg.ToString() , ex , cancellationToken);
        }
        catch (TimeoutException ex)
        {
            msg.Append("The database operation timed out. Please ensure the database is responsive.");
            await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
            throw new Exception(msg.ToString() , ex);
        }
        catch (Exception ex)
        {
            // Handle specific database errors with user-friendly messages
            msg = BuildExceptionMessage(entity , ex);

            await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
            throw new Exception(msg.ToString() , ex);
        }

        return msg.ToString();
    }
    /// <summary>
    /// Builds a user-friendly exception message based on the type of database error
    /// </summary>
    /// <param name="entity">The entity being processed</param>
    /// <param name="ex">The exception that was thrown</param>
    /// <returns>A formatted error message</returns>
    private StringBuilder BuildExceptionMessage(EntityBase entity , Exception ex)
    {
        var msg = new StringBuilder();
        string entityName = entity is AppBaseEntity appEntity ? appEntity.Name ?? entity.GetType().Name : entity.GetType().Name;

        // Handle date validation errors
        if (ex.Message.Contains("1753"))
        {
            msg.Append($"{PropertyFinderUtility.PrintInvalidDateTimeProperties(entity)}\n");
        }

        // Handle string truncation errors
        if (ex.InnerException != null && ex.InnerException.Message.Contains("String or binary data would be truncated"))
        {
            if (AppBaseEntity.RightToLeft)
                msg.Append("أحد المدخلات طويل جدًا ، يرجى جعله أقصر \n");
            else
                msg.Append("One of the strings is too long. Please make it shorter \n");
        }

        // Handle reference constraint errors
        if (ex.InnerException != null && ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE"))
        {
            int start = ex.InnerException.Message.IndexOf("dbo.") + 4;
            if (start >= 4)
            {
                int end = ex.InnerException.Message.IndexOf("VO");
                if (end > start)
                {
                    string referencedTable = ex.InnerException.Message.Substring(start , end - start);
                    msg.Append($"The transaction has reference in {referencedTable}");
                }
            }
        }
        // Handle duplicate key errors
        else if (ex.InnerException != null && ex.InnerException.Message.Contains("duplicate key"))
        {
            int start = ex.InnerException.Message.IndexOf("Cannot");
            if (start >= 0)
            {
                int end = ex.InnerException.Message.IndexOf("VO");
                if (end > start)
                {
                    string duplicateKey = ex.InnerException.Message.Substring(start , end - start);
                    duplicateKey = duplicateKey.Replace("dbo." , "");
                    msg.Append(duplicateKey);
                }
                else
                {
                    msg.Append("A duplicate key error occurred");
                }
            }
        }

        // Add general error message if nothing specific was added yet
        if (msg.Length == 0)
        {
            if (!string.IsNullOrEmpty(ex.Message))
            {
                msg.Append(ex.Message);
            }
            else if (ex.InnerException != null)
            {
                msg.Append(ex.InnerException.Message);
            }
            else
            {
                // Default error message
                if (AppBaseEntity.RightToLeft)
                    msg.Append("للاسف!! لم تتم عملية الحفظ \n");
                else
                    msg.Append("Unfortunately! The operation could not be completed\n");
            }
        }

        // Add entity name context if not already present in message
        if (!string.IsNullOrEmpty(entityName) && !msg.ToString().Contains(entityName))
        {
            msg.Insert(0 , AppBaseEntity.RightToLeft
            ? $"{entityName}: للاسف!! لم تتم عملية الحفظ \n"
            : $"{entityName}: Operation failed \n");
        }

        return msg;
    }
    /// <summary>
    /// Extracts a user-friendly reference constraint violation message from an exception
    /// </summary>
    /// <param name="ex">The exception containing reference constraint information</param>
    /// <returns>A formatted error message string</returns>
    private string ExtractReferenceMessage(Exception ex)
    {
        if (ex?.InnerException == null || string.IsNullOrEmpty(ex.InnerException.Message))
            return "Unknown reference constraint";

        string message = ex.InnerException.Message;
        string result = "";

        try
        {
            // Handle SQL Server reference constraint messages
            if (message.Contains("dbo."))
            {
                int startIndex = message.IndexOf("dbo.") + 4;
                if (startIndex > 4)
                {
                    string remaining = message.Substring(startIndex);
                    // Try to extract the table name (often ends with "VO")
                    int endIndex = remaining.IndexOf("VO");
                    if (endIndex > 0)
                    {
                        result = remaining.Substring(0 , endIndex);
                    }
                    else
                    {
                        // Alternative approach - look for common delimiters
                        endIndex = remaining.IndexOfAny(new[] { ' ' , '.' , ')' , ',' });
                        if (endIndex > 0)
                        {
                            result = remaining.Substring(0 , endIndex);
                        }
                        else
                        {
                            // If no proper delimiter found, use the whole remaining text
                            result = remaining;
                        }
                    }

                    // Clean up the extracted name
                    result = result.Replace("VO" , "").Trim();
                }
            }
            // Handle MySQL reference constraint messages
            else if (message.Contains("FOREIGN KEY constraint"))
            {
                int startIndex = message.IndexOf("FOREIGN KEY constraint") + 22;
                if (startIndex > 22)
                {
                    string remaining = message.Substring(startIndex);
                    // Try to find table name reference
                    int tableIndex = remaining.IndexOf("table");
                    if (tableIndex > 0)
                    {
                        result = remaining.Substring(tableIndex + 6).Trim();
                        // Get just the table name
                        int endIndex = result.IndexOfAny(new[] { ' ' , '(' , '.' });
                        if (endIndex > 0)
                        {
                            result = result.Substring(0 , endIndex);
                        }
                    }
                }
            }

            // If we couldn't extract anything specific, return a cleaned up version of the message
            if (string.IsNullOrEmpty(result))
            {
                // Remove common technical prefixes
                result = message.Replace("The DELETE statement conflicted with the REFERENCE constraint" , "")
                .Replace("Cannot delete or update a parent row" , "")
                .Trim();

                // Limit the length to avoid overly long messages
                if (result.Length > 200)
                    result = result.Substring(0 , 197) + "...";
            }
        }
        catch
        {
            // If anything goes wrong in parsing, return a safe default
            result = "Referenced by another entity";
        }

        return result;
    }
    /// <summary>
    /// Merges multiple entities in a single transaction
    /// </summary>
    public async Task<string> MergeList(IEnumerable<EntityBase> entities , int batchSize = DEFAULT_BATCH_SIZE , CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            return "No entities to save";

        var msg = new StringBuilder();
        EntityBase lastEntityWork = null;

        try
        {
            SessionClear();

            // Get the count before entering the transaction - this ensures we have it even in case of partial failure
            int totalEntities = entities.Count();

            // Use the transaction helper for the database operations
            await ExecuteInTransactionAsync<bool>(
            async (session , ct) =>
            {
                int count = 0;
                foreach (var entity in entities)
                {
                    // Check for cancellation
                    ct.ThrowIfCancellationRequested();

                    // Keep track of the last entity for reporting
                    lastEntityWork = entity;

                    // Set user info and merge
                    entity.UserEdited = AppBaseEntity.Instance.UserCurrentStatic;
                    await session.MergeAsync(entity , ct).ConfigureAwait(false);

                    // Flush periodically to manage memory usage
                    if (++count % batchSize == 0)
                    {
                        await session.FlushAsync(ct).ConfigureAwait(false);
                        session.Clear();
                    }
                }

                // Final flush to ensure all changes are committed
                await session.FlushAsync(ct).ConfigureAwait(false);

                return true;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);

            // Create a user tracker for the operation after successful commit
            if (lastEntityWork != null)
            {
                var userTracker = new UserTrackerVO
                {
                    TransationType = ReminderVO.TriggerKinds.Edit ,
                    Comments = $"Updated {totalEntities} {lastEntityWork.GetType().Name} entities"
                };
                await afterSavingAsync(userTracker , lastEntityWork).ConfigureAwait(false);
            }

            msg.Append(AppBaseEntity.RightToLeft
            ? "تم حفظ القائمة بنجاح"
            : "The list was saved successfully");
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation specifically
            msg.Append(AppBaseEntity.RightToLeft
            ? "تم إلغاء العملية"
            : "Operation was cancelled");
        }
        catch (Exception ex)
        {
            // Handle date validation errors
            if (ex.Message.Contains("1753"))
            {
                msg.Append($"{PropertyFinderUtility.PrintInvalidDateTimeProperties(entities.FirstOrDefault())}\n");
            }

            // Format error message
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{(lastEntityWork != null ? ((AppBaseEntity)lastEntityWork).Name : "Entity")} للاسف!! لم تتم عملية الحفظ \n{ex.Message}"
            : $"{(lastEntityWork != null ? ((AppBaseEntity)lastEntityWork).Name : "Entity")} was NOT saved successfully \n{ex.Message}");

            if (lastEntityWork != null)
            {
                await loggErrorMessage(lastEntityWork , msg.ToString() , ex).ConfigureAwait(false);
            }

            throw new Exception(msg.ToString() , ex);
        }

        return msg.ToString();
    }

    /// <summary>
    /// Saves or updates an entity
    /// </summary>
    public async Task<string> PersistAsync(EntityBase entity , CancellationToken cancellationToken = default)
    {
        if (entity == null)
            return "Entity cannot be null";

        var msg = new StringBuilder();

        try
        {
            // Set user tracking information before transaction
            entity.UserEdited = AppBaseEntity.Instance.UserCurrentStatic;

            // Determine operation type
            var userTracker = new UserTrackerVO
            {
                TransationType = entity.Id != Guid.Empty
            ? ReminderVO.TriggerKinds.Edit
            : ReminderVO.TriggerKinds.Add
            };

            // Execute the save/update operation within a transaction
            await ExecuteInTransactionAsync<EntityBase>(
            async (session , ct) =>
            {
                // Save or update entity
                await session.SaveOrUpdateAsync(entity , ct).ConfigureAwait(false);
                await session.FlushAsync(ct).ConfigureAwait(false);
                return entity;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);

            // Process tracker after successful transaction
            await afterSavingAsync(userTracker , entity).ConfigureAwait(false);

            // Build success message
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name} تم الحفظ بنجاح"
            : $"{((AppBaseEntity)entity).Name} was saved successfully");
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
            msg.Append(AppBaseEntity.RightToLeft
            ? "تم إلغاء العملية"
            : "Operation was cancelled");
        }
        catch (Exception ex)
        {
            // Format error message
            msg.Append(AppBaseEntity.RightToLeft
            ? $"للاسف!! لم تتم عملية الحفظ \n{ex.Message}"
            : $"Save operation failed \n{ex.Message}");

            await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
            throw new Exception(msg.ToString() , ex);
        }

        return msg.ToString();
    }

    /// <summary>
    /// Saves or updates a list of entities in batches
    /// </summary>
    public async Task<string> PersistList(IEnumerable<EntityBase> entities , int batchSize = DEFAULT_BATCH_SIZE , CancellationToken cancellationToken = default)
    {
        if (entities == null || !entities.Any())
            return "No entities to save";

        var msg = new StringBuilder();
        var trackerList = new List<(UserTrackerVO Tracker , EntityBase Entity)>();

        try
        {
            SessionClear();

            // Execute the save/update operations within a transaction
            await ExecuteInTransactionAsync<bool>(
            async (session , ct) =>
            {
                int count = 0;

                foreach (var entity in entities)
                {
                    // Check for cancellation
                    ct.ThrowIfCancellationRequested();

                    // Set user tracking information
                    entity.UserEdited = AppBaseEntity.Instance.UserCurrentStatic;

                    // Save or update entity
                    await session.SaveOrUpdateAsync(entity , ct).ConfigureAwait(false);

                    // Create and store tracker for later processing
                    var userTracker = new UserTrackerVO
                    {
                        TransationType = entity.Id != Guid.Empty
    ? ReminderVO.TriggerKinds.Edit
    : ReminderVO.TriggerKinds.Add
                    };
                    trackerList.Add((userTracker , entity));

                    // Flush in batches to manage memory
                    if (++count % batchSize == 0)
                    {
                        await session.FlushAsync(ct).ConfigureAwait(false);
                        session.Clear();
                    }
                }

                // Final flush to ensure all entities are processed
                await session.FlushAsync(ct).ConfigureAwait(false);

                return true;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);

            // Process trackers after successful transaction
            foreach (var (tracker , entity) in trackerList)
            {
                await endEditUserTracker(tracker , entity).ConfigureAwait(false);
            }

            // Build success message
            msg.Append(AppBaseEntity.RightToLeft
            ? "تم الحفظ بنجاح"
            : "The list was saved successfully");
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
            msg.Append(AppBaseEntity.RightToLeft
            ? "تم إلغاء العملية"
            : "Operation was cancelled");
        }
        catch (Exception ex)
        {
            // Handle date validation errors
            var firstEntity = entities.FirstOrDefault();
            if (ex.Message.Contains("1753") && firstEntity != null)
            {
                msg.Append($"{PropertyFinderUtility.PrintInvalidDateTimeProperties(firstEntity)}\n");
            }

            // Add exception message
            msg.Append(ex.Message);

            // Log the error
            if (firstEntity != null)
            {
                await loggErrorMessage(firstEntity , msg.ToString() , ex).ConfigureAwait(false);
            }

            throw new Exception(msg.ToString() , ex);
        }

        return msg.ToString();
    }

    /// <summary>
    /// Updates a single field on an entity
    /// </summary>
    public async Task<string> UpdateAsync(EntityBase entity , string fieldName , object newValue , CancellationToken cancellationToken = default)
    {
        if (entity == null)
            return "Entity cannot be null";

        var msg = new StringBuilder();
        var userTracker = new UserTrackerVO { TransationType = ReminderVO.TriggerKinds.Edit };

        try
        {
            string entityName = entity.GetType().Name.Replace("Proxy" , "");

            // Inside your UpdateAsync method, replace the ExecuteInTransactionAsync call with:
            int rowsAffected = await ExecuteUpdateWithSessionAsync<int>(
            async (session , ct) =>
            {
                // Create and execute HQL update query
                var query = session.CreateQuery($"UPDATE {entityName} SET {fieldName}=:newValue WHERE Id=:id")
.SetParameter("newValue" , newValue)
.SetParameter("id" , entity.Id);

                int result = await query.ExecuteUpdateAsync(ct).ConfigureAwait(false);
                return result;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);
            // Process tracker after successful transaction
            await afterSavingAsync(userTracker , entity).ConfigureAwait(false);

            // Build success message
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name}\n تم الحفظ بنجاح"
            : $"{((AppBaseEntity)entity).Name} was updated successfully");
        }
        catch (Exception ex)
        {
            // Fall back to full entity merge if the update fails
            try
            {
                msg.Clear();
                msg.Append(await MergeAsync(entity , cancellationToken).ConfigureAwait(false));
            }
            catch (Exception mergeEx)
            {
                msg.Clear();
                msg.Append(AppBaseEntity.RightToLeft
                ? $"فشلت عملية التحديث: {mergeEx.Message}"
                : $"Update operation failed: {mergeEx.Message}");

                await loggErrorMessage(entity , msg.ToString() , mergeEx).ConfigureAwait(false);
                throw new Exception(msg.ToString() , mergeEx);
            }
        }

        return msg.ToString();
    }

    /// <summary>
    /// Updates entities matching a where clause
    /// </summary>
    public async Task<string> UpdateWhereAsync(EntityBase entity , string fieldName , object newValue , string whereClause , CancellationToken cancellationToken = default)
    {
        if (entity == null || string.IsNullOrEmpty(whereClause))
            return "Entity or where clause cannot be null";

        var msg = new StringBuilder();
        var userTracker = new UserTrackerVO { TransationType = ReminderVO.TriggerKinds.Edit };

        try
        {
            // Execute the bulk update operation within a transaction
            int affectedRows = await ExecuteInTransactionAsync<int>(
            async (session , ct) =>
            {
                string entityName = entity.GetType().Name.Replace("Proxy" , "");

                // Create and execute HQL update query
                var query = session.CreateQuery($"UPDATE {entityName} SET {fieldName}=:newValue WHERE {whereClause}")
.SetParameter("newValue" , newValue);

                int rows = await query.ExecuteUpdateAsync(ct).ConfigureAwait(false);
                return rows;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);

            // Add comment about affected rows
            userTracker.Comments = $"Bulk update of {affectedRows} {entity.GetType().Name.Replace("Proxy" , "")} records where {whereClause}";

            // Process tracker after successful transaction (only once)
            await afterSavingAsync(userTracker , entity).ConfigureAwait(false);

            // Build success message
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name}\n تم التحديث بنجاح"
            : $"{((AppBaseEntity)entity).Name} was updated successfully");
        }
        catch (Exception ex)
        {
            // Handle different types of failures
            if (ex.Message.Contains("syntax error"))
            {
                try
                {
                    // Fall back to full entity merge if the update fails due to syntax error
                    return await MergeAsync(entity , cancellationToken).ConfigureAwait(false);
                }
                catch (Exception mergeEx)
                {
                    msg.Clear();
                    msg.Append(AppBaseEntity.RightToLeft
                    ? $"فشلت عملية التحديث: {mergeEx.Message}"
                    : $"Update operation failed: {mergeEx.Message}");

                    await loggErrorMessage(entity , msg.ToString() , mergeEx).ConfigureAwait(false);
                    throw new Exception(msg.ToString() , mergeEx);
                }
            }
            else
            {
                // Handle other exceptions
                msg.Append(AppBaseEntity.RightToLeft
                ? $"{((AppBaseEntity)entity).Name}\n فشلت عملية التحديث: {ex.Message}"
                : $"{((AppBaseEntity)entity).Name} update failed: {ex.Message}");

                await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
                throw new Exception(msg.ToString() , ex);
            }
        }

        return msg.ToString();
    }

    /// <summary>
    /// Deletes entities that match the specified where clause.
    /// </summary>
    /// <param name="entity">The entity type reference used for the deletion</param>
    /// <param name="whereClause">The where clause to identify which entities to delete</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A string message indicating the result of the operation</returns>
    /// <remarks>
    /// Last updated by: namer315
    /// Last update: 2025-03-16 20:52:54
    /// </remarks>
    public async Task<string> DeleteWhereAsync(EntityBase entity , string whereClause , CancellationToken cancellationToken = default)
    {
        if (entity == null || string.IsNullOrEmpty(whereClause))
            return "Entity or where clause cannot be null";

        StringBuilder msg = new StringBuilder();

        try
        {
            // Execute the bulk delete operation within a transaction
            int affectedRows = await ExecuteInTransactionAsync<int>(
            async (session , ct) =>
            {
                string entityName = entity.GetType().Name.Replace("Proxy" , "");

                // Create and execute HQL delete query
                var query = session.CreateQuery($"DELETE FROM {entityName} WHERE {whereClause}");
                int rows = await query.ExecuteUpdateAsync(ct).ConfigureAwait(false);
                return rows;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);

            // Create a tracker for audit purposes
            var userTracker = new UserTrackerVO
            {
                TransationType = ReminderVO.TriggerKinds.Delete ,
                Comments = $"Bulk delete of {affectedRows} {entity.GetType().Name.Replace("Proxy" , "")} records where {whereClause}"
            };

            // Process the tracker
            await endEditUserTracker(userTracker , entity).ConfigureAwait(false);

            // Build success message
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name}\n تم الحذف بنجاح"
            : $"{((AppBaseEntity)entity).Name} was deleted successfully");
        }
        catch (Exception ex)
        {
            // Handle exceptions
            msg.Append(AppBaseEntity.RightToLeft
            ? $"{((AppBaseEntity)entity).Name}\n فشلت عملية الحذف: {ex.Message}"
            : $"{((AppBaseEntity)entity).Name} deletion failed: {ex.Message}");

            await loggErrorMessage(entity , msg.ToString() , ex).ConfigureAwait(false);
            throw new Exception(msg.ToString() , ex);
        }

        return msg.ToString();
    }
    private async Task afterSavingAsync(UserTrackerVO userTracker , EntityBase entity)
    {
        try
        {
            await endEditUserTracker(userTracker , entity);
            await ReminderEmail.Instance.ReminderFireAsync(userTracker.TransationType , entity);
            await ReminderSMS.Instance.ReminderFire(userTracker.TransationType , entity);
        }
        catch (Exception ex)
        {
            await loggErrorMessage(((AppBaseEntity)entity) , "Email OR SMS" , ex);
        }
    }

    #region UserTracker

    public static void SessionClear()
    {
        if (_session.Value != null && _session.Value.IsOpen)
        {
            _session.Value.Clear();
            if (_statelessSession != null)
                _statelessSession.Dispose();
            //_session.Value = null;
            //_session.Value.Reconnect();
        }
    }

    public static void SessionClose()
    {
        if (_session.Value != null && _session.Value.IsOpen)
        {
            _session.Value.Clear();
            _session.Value.Close();
        }
    }

    public static void Evict(EntityBase entity)
    {
        if (_session.Value != null && _session.Value.IsOpen)
        {
            _session.Value.Evict(entity);
        }
    }

    /// <summary>
    /// Processes and saves user tracking information after a database operation.
    /// </summary>
    /// <param name="userTracker">The user tracker object with operation details</param>
    /// <param name="entity">The entity being tracked</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <remarks>
    /// Last updated by: namer315
    /// Last update: 2025-03-16 21:26:42
    /// </remarks>
    private async Task endEditUserTracker(UserTrackerVO userTracker , EntityBase entity , CancellationToken cancellationToken = default)
    {
        try
        {
            // Early validation
            if (AppBaseEntity.Instance.UserCurrentStatic == null || entity is null)
                return;

            // Set entity ID based on entity type
            if (entity.Id != Guid.Empty)
                userTracker.EntityId = entity.Id;
            else if (entity is InvoiceVO || entity is OrderVO || entity is VoucherVO)
                userTracker.EntityId = AuditableListener.entityId;

            if (userTracker.EntityId == Guid.Empty)
                return;

            // Configure tracker properties
            userTracker.OwnerTypeName = entity.ToString();
            var propertyInfo = entity.GetType().GetProperty("Code");
            if (propertyInfo != null)
                userTracker.Code = Convert.ToString(propertyInfo.GetValue(entity));

            userTracker.UserEdited = AppBaseEntity.Instance.UserCurrentStatic;
            userTracker.LocalName = ((AppBaseEntity)entity).LocalName;
            userTracker.EnglishName = ((AppBaseEntity)entity).EnglishName;
            userTracker.ClassName = entity.GetType().Name;
            userTracker.MachineName = System.Environment.MachineName;

            // Set comments based on operation type
            string currentTime = DateTime.Now.ToString();
            string userName = AppBaseEntity.Instance.UserCurrentStatic.Name;
            string entityName = ((AppBaseEntity)entity).Name;

            switch (userTracker.TransationType)
            {
                case ReminderVO.TriggerKinds.Add:
                    userTracker.Comments = $"User Name : {userName} / Added : {entityName} / In Date : {currentTime}";
                    break;
                case ReminderVO.TriggerKinds.Edit:
                    userTracker.Comments = $"User Name : {userName} / Edited : {entityName} / In Date : {currentTime}";
                    break;
                case ReminderVO.TriggerKinds.Delete:
                    userTracker.Comments = $"User Name : {userName} / Deleted : {entityName} / ID : {entity.Id} / In Date : {currentTime}";
                    break;
            }

            // Process dirty properties changes
            if (AuditableListener.DirtyProperties != null)
            {
                userTracker.Changes = AuditableListener.DirtyProperties.ToString();
                // Limit userTracker.Changes to 4000 characters
                if (userTracker.Changes.Length > 4000)
                    userTracker.Changes = userTracker.Changes.Substring(0 , 4000);
                AuditableListener.DirtyProperties.Clear();
            }

            // Save the tracker using ExecuteInTransactionAsync
            await ExecuteInTransactionAsync<bool>(
            async (session , ct) =>
            {
                await session.SaveAsync(userTracker , ct).ConfigureAwait(false);
                return true;
            } ,
            cancellationToken ,
            IsolationLevel.ReadCommitted
            ).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Silent exception handling as in original method
            // Consider adding proper logging here instead of swallowing exceptions
            // _logger.ErrorException($"Error tracking user activity: {ex.Message}", ex);
        }
    }

    #endregion UserTracker

    public async Task loggErrorMessage(EntityBase entity , string msg , Exception ex)
    {
        try
        {
            //Logger _logger = LogManager.GetCurrentClassLogger();
            CurrentLogger.Instance.Error($" User : {AppBaseEntity.Instance.UserCurrentStatic?.Name} \r\n {msg}  \r\n Entity Name :{entity.GetType().Name} \r\n Object Id : {((AppBaseEntity)entity).Id} \r\n Object Name: {((AppBaseEntity)entity).Name} \r\n\r\n {ex.Message}  :\r\n {ex.InnerException} \r\n\r\n\r\n\r\n" , ex);
            await EmailHelper.EmailTroubleShoot(entity.GetType().Name , ex.Message , CurrentLogger.Instance);
            if (ex.InnerException != null && (ex.InnerException.Message.Contains("Invalid column name") || ex.InnerException.Message.Contains("Invalid object name")))
            {
                await Connection.DataBaseUpdate(Connection.CurrentConnection);
            }
        }
        catch (Exception ex1)
        {
        }

    }
    /// <summary>
    /// Executes a field update operation within a session asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the result</typeparam>
    /// <param name="action">The action to perform with the session</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="isolationLevel">Transaction isolation level (defaults to ReadCommitted)</param>
    /// <returns>Result of the action</returns>
    public async Task<T> ExecuteUpdateWithSessionAsync<T>(
    Func<ISession , CancellationToken , Task<T>> action ,
    CancellationToken cancellationToken = default ,
    IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        using (var session = SessionFactoryGenerator.SessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction(isolationLevel))
        {
            try
            {
                var result = await action(session , cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                if (transaction.IsActive)
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);

                // Log exception here if needed
                throw;
            }
        }
    }
    */
    /*
            List<(string, object)> paramList = new List<(string, object)>();
           paramList.Add(("post", true));
           paramList.Add(("fromDate", fromDate));
           paramList.Add(("toDate", toDate));

           var result = await RunHQLAsync<object[]>(stockTransferBranchQuery, paramList);
           */
    /// <summary>
    /// this is ONLY for reports
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task<T> ExecuteWithSessionAsync<T>(Func<ISession , Task<T>> action)
    {
        using (var session = SessionFactoryGenerator.SessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            try
            {
                var result = await action(session);
                return result;
            }
            catch (Exception ex)
            {
                if (transaction.IsActive)
                    await transaction.RollbackAsync();

                // Log exception
                throw;
            }
        }
    }
    public async Task<IList> RunHQLAsync(string hqlQuery , IList<(string , object)> parameters)
    {
        using (var currentSession = SessionFactoryGenerator.SessionFactory.OpenSession())
        using (var transaction = currentSession.BeginTransaction(isolationLevel: IsolationLevel.ReadUncommitted))
        {
            IQuery q = currentSession.CreateQuery(hqlQuery);

            foreach (var parameter in parameters)
            {
                if (parameter.Item2 is IEnumerable && !(parameter.Item2 is string))
                {
                    // If the parameter is a list, use SetParameterList
                    q.SetParameterList(parameter.Item1 , (IEnumerable)parameter.Item2);
                }
                else
                {
                    // Otherwise, use SetParameter
                    q.SetParameter(parameter.Item1 , parameter.Item2);
                }
            }

            // Execute the query asynchronously and return the result as a List
            return await q.ListAsync();
        }
    }

    public async Task<IList<T>> RunHQLAsync<T>(string hqlQuery , List<(string , object)> parameters)
    {
        return await RunHQLAsync<T>(hqlQuery , parameters , false);
    }

    public async Task<IList<T>> RunHQLAsync<T>(string hqlQuery , List<(string , object)> parameters , bool distinctRootEntity)
    {
        using (var currentSession = SessionFactoryGenerator.SessionFactory.OpenSession())
        using (var transaction = currentSession.BeginTransaction(isolationLevel: IsolationLevel.ReadUncommitted))
        {
            IQuery q = currentSession.CreateQuery(hqlQuery);

            foreach (var parameter in parameters)
            {
                if (parameter.Item2 is IEnumerable && !(parameter.Item2 is string))
                {
                    // If the parameter is a list, use SetParameterList
                    q.SetParameterList(parameter.Item1 , (IEnumerable)parameter.Item2);
                }
                else
                {
                    // Otherwise, use SetParameter
                    q.SetParameter(parameter.Item1 , parameter.Item2);
                }
            }

            if (distinctRootEntity)
            {
                q.SetResultTransformer(new DistinctRootEntityResultTransformer());
            }

            // Execute the query asynchronously and return the result as a List
            return await q.ListAsync<T>();
        }
    }
    /*
           */
    /// <summary>
    /// var queries = new List<(string hqlQuery, List<(string, object)> parameters)>
    ///    {
    ///(hqlQuery1, parameters1),
    ///(hqlQuery2, parameters2),
    /// Add more queries and parameters as needed
    ///};

    ///var yourDataAccess = new YourDataAccessClass();
    ///var results = await yourDataAccess.RunMultipleHQLAsync<object[]>(queries);
    /// <summary>
    public async Task<List<IList>> RunMultipleHQLAsync<T>(IEnumerable<(string hqlQuery , List<(string , object)> parameters)> queries)
    {
        List<IList> results = new List<IList>();
        using (var currentSession = SessionFactoryGenerator.SessionFactory.OpenSession())
        {
            using (var transaction = currentSession.BeginTransaction(isolationLevel: System.Data.IsolationLevel.ReadUncommitted))
            {
                var multiQuery = currentSession.CreateMultiQuery();
                foreach (var query in queries)
                {
                    IQuery q = currentSession.CreateQuery(query.hqlQuery);
                    foreach (var parameter in query.parameters)
                    {
                        SetQueryParameter(q , parameter.Item1 , parameter.Item2);
                    }
                    multiQuery.Add(q);
                }
                // Execute the multi-query asynchronously and add the results to the list
                var multiResults = await multiQuery.ListAsync();
                foreach (var multiResult in multiResults)
                {
                    results.Add((IList)multiResult);
                }
            }
        }
        return results;
    }

    private void SetQueryParameter(IQuery query , string parameterName , object parameterValue)
    {
        switch (parameterValue)
        {
            case IList list when list.Count > 0:
                query.SetParameterList(parameterName , list);
                break;
            case IEnumerable<Guid> guidList:
                query.SetParameterList(parameterName , guidList);
                break;
            case IEnumerable enumerable when !(parameterValue is string):
                query.SetParameterList(parameterName , enumerable.Cast<object>());
                break;
            default:
                query.SetParameter(parameterName , parameterValue);
                break;
        }
    }
}

// Helper class for repository exceptions
public class RepositoryException : Exception
{
    public string EntityType { get; }
    public Guid? EntityId { get; }
    public string Operation { get; }

    public RepositoryException(
    string message ,
    string entityType ,
    Guid? entityId ,
    string operation ,
    Exception innerException = null)
    : base(message , innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
        Operation = operation;

        //// Add additional logging with user context
        //var userId = AppBaseEntity.Instance.UserCurrentStatic?.Id;
        //var userName = AppBaseEntity.Instance.UserCurrentStatic?.Name;

        // You could add structured logging here
        //CurrentLogger.Instance.Error(
        //$"Repository operation failed at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n" +
        //$"User: {userName} (ID: {userId})\n" +
        //$"Operation: {operation}\n" +
        //$"Entity Type: {entityType}\n" +
        //$"Entity ID: {entityId}\n" +
        //$"Message: {message}" ,
        //innerException
        //);
    }
}
