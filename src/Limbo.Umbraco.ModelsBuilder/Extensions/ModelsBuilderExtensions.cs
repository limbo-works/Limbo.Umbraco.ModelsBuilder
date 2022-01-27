namespace Limbo.Umbraco.ModelsBuilder.Extensions {
    
    internal static class ModelsBuilderExtensions {

        public static bool HasValue<T>(this T input) {
            return HasValue<T>(input, out _);
        }
        

        public static bool HasValue<T>(this T input, out T result) {

            result = input;

            if (input == null) return false;

            // TODO: Validate strings, numbers and enums

            return true;

        }


    }

}