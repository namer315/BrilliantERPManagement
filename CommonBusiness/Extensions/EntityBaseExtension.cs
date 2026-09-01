using CommonData.DAO;
using CommonData.Extensions;
using CommonData.VO;
using System.Text.RegularExpressions;

namespace CommonBusiness.Extensions;

public static class EntityBaseExtension
{
    public static async Task<T> GetNextCodeNumber<T>(
        this RepositoryBase repository ,
        string whereCondition = "" ,
        string leftJoin = "")
        where T : EntityBaseWithCode, new()
    {
        // Get the last entity (Code + Number)
        T lastEntity = await repository.GetLastCodeNumber<T>(whereCondition);

        // Start with incremented values
        T nextEntity = new T
        {
            Number = lastEntity.Number + 1 ,
            Code = GetLastDigit(lastEntity.Code)
        };

        // Loop until Code is unique
        while (await repository.IsCodeExists<T>(nextEntity.Code , nextEntity.Id , whereCondition , leftJoin))
        {
            nextEntity.Code = GetLastDigit(nextEntity.Code);
        }

        // Loop until Number is unique
        while (await repository.IsNumberExists<T>(nextEntity.Number , nextEntity.Id , whereCondition , leftJoin))
        {
            nextEntity.Number++;
        }

        return nextEntity;
    }

    private static string GetLastDigit(string code)
    {
        //"A1B2c3d456"
        if (code == null || string.IsNullOrEmpty(code))
            return "0001";
        try
        {
            // Find all numbers in the string
            MatchCollection matches = Regex.Matches(code , @"\d+");

            if (matches.Count == 0)
                return code + "0001"; // No numbers found, append default

            // Get the last number found
            Match lastMatch = matches[matches.Count - 1];
            long number = Convert.ToInt64(lastMatch.Value);
            number++; // Increment the number

            // Replace the last number with the incremented value
            string result = code.Substring(0 , lastMatch.Index) +
                            number.ToString() +
                            code.Substring(lastMatch.Index + lastMatch.Length);

            return result;
        }
        catch (Exception ex)
        {
            return "0001";
        }
    }
}
