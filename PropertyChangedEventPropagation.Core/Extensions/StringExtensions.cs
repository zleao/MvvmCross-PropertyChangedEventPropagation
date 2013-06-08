
namespace PropertyChangedEventPropagation.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether the specified source string is null or empty.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if the specified source string is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        /// <summary>
        /// Formats the specified source template.
        /// </summary>
        /// <param name="sourceTemplate">The source template.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static string FormatTemplate(this string sourceTemplate, params object[] args)
        {
            if (string.IsNullOrEmpty(sourceTemplate))
                return sourceTemplate ?? string.Empty;

            return string.Format(sourceTemplate, args);
        }
    }
}
