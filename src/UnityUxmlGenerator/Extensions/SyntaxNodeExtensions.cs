using Microsoft.CodeAnalysis;

namespace UnityUxmlGenerator.Extensions;

internal static class SyntaxNodeExtensions
{
    public static T? GetParent<T>(this SyntaxNode syntaxNode)
    {
        var parent = syntaxNode.Parent;
            
        while (parent != null)
        {
            if (parent is T result)
            {
                return result;
            }

            parent = parent.Parent;
        }

        return default;
    }
}