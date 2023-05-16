using System.Text;

namespace UnityUxmlGenerator.Extensions;

internal static class StringExtensions
{
    public static string ToPrivateFieldName(this string propertyName)
    {
        return string.Concat("_", char.ToLower(propertyName[0]), propertyName.Substring(1));
    }

    public static string ToDashCase(this string propertyName)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(char.ToLower(propertyName[0]));

        for (var i = 1; i < propertyName.Length; i++)
        {
            if (char.IsUpper(propertyName[i]))
            {
                stringBuilder.Append("-");
                stringBuilder.Append(char.ToLower(propertyName[i]));
            }
            else
            {
                stringBuilder.Append(propertyName[i]);
            }
        }

        return stringBuilder.ToString();
    }
}