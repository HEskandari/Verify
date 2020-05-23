﻿using System.Collections.Generic;

namespace Verify
{
    public static partial class SharedVerifySettings
    {
        static Dictionary<string, Compare> comparers = new Dictionary<string, Compare>();

        internal static bool TryGetComparer(string extension, out Compare comparer)
        {
            return comparers.TryGetValue(extension, out comparer);
        }

        public static void RegisterComparer(string extension, Compare compare)
        {
            Guard.AgainstNull(compare, nameof(compare));
            Guard.AgainstBadExtension(extension, nameof(extension));
            comparers[extension] = compare;
        }
    }
}