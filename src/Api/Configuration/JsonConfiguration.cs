using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ITExpert.OcrService.Configuration
{
    public static class JsonConfiguration
    {
        public static readonly JsonSerializerSettings Instance = CreateConfiguredSettings();

        private static JsonSerializerSettings CreateConfiguredSettings()
        {
            var settings = new JsonSerializerSettings();
            Configure(settings);
            return settings;
        }
        
        public static void Configure(this JsonSerializerSettings settings)
        {
            settings.Culture = CultureInfo.InvariantCulture;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new StringEnumConverter {CamelCaseText = true, AllowIntegerValues = false});
        }
    }
}