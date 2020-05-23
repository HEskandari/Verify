﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verify;

partial class InnerVerifier
{
    public async Task Verify<T>(T target, VerifySettings? settings = null)
    {
        Guard.AgainstNull(target, nameof(target));
        settings = settings.OrDefault();

        if (SharedVerifySettings.TryGetConverter(
            target,
            settings.extension,
            out var converter))
        {
            var converterSettings = GetConverterSettings(settings, converter);
            var result = await converter.Conversion(target!, converterSettings);
            await VerifyBinary(result.Streams, converterSettings, result.Info);
            return;
        }

        if (target is Stream stream)
        {
            await VerifyStream(settings, stream);
            return;
        }

        if (typeof(T).ImplementsStreamEnumerable())
        {
            var enumerable = (IEnumerable) target!;
            await VerifyBinary(enumerable.Cast<Stream>(), settings, null);
            return;
        }

        var formatJson = JsonFormatter.AsJson(
            target,
            settings.serialization.currentSettings,
            settings.IsNewLineEscapingDisabled);
        var s = formatJson.ToString();
        await Verify(formatJson, settings);
    }

    static VerifySettings GetConverterSettings(VerifySettings settings, TypeConverter converter)
    {
        if (converter.ToExtension != null)
        {
            var converterSettings = new VerifySettings(settings);
            converterSettings.UseExtension(converter.ToExtension);
            return converterSettings;
        }

        if (settings.HasExtension())
        {
            return settings;
        }

        throw new Exception("No extension defined.");
    }
}