﻿using System.IO;
using System.Text;
using Sparrow.Json;
using System.Collections.Generic;

namespace Raven.Server.Documents.Replication
{
    public static class ChangeVectorExtensions
    {              
        public static string SerializeVector(this ChangeVectorEntry[] self)
        {
            if (self == null)
                return null;

            var sb = new StringBuilder();
            for (int i = 0; i < self.Length; i++)
            {
                if (i != 0)
                    sb.Append(", ");
                self[i].Append(sb);
            }
            return sb.ToString();
        }
        public static string SerializeVector(this List<ChangeVectorEntry> self)
        {
            if (self == null)
                return null;

            var sb = new StringBuilder();
            for (int i = 0; i < self.Count; i++)
            {
                if (i != 0)
                    sb.Append(", ");
                self[i].Append(sb);
            }
            return sb.ToString();
        }

        public static void ToBase26(StringBuilder sb, int tag)
        {
            do
            {
                var reminder = tag % 26;
                sb.Append((char)('A' + reminder));
                tag /= 26;
            } while (tag != 0);
        }

        public static int FromBase26(string tag)
        {
            //TODO: validation of valid chars
            var val = 0;
            for (int i = 0; i < tag.Length; i++)
            {
                val *= 26;
                val += (tag[i] - 'A');
            }
            return val;
        }
    }
}