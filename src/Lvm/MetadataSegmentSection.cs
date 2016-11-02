//
// Copyright (c) 2016, Bianco Veigel
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//


namespace DiscUtils.Lvm
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    internal class MetadataSegmentSection
    {
        public string Name;
        public ulong StartExtent;
        public ulong ExtentCount;
        public SegmentType Type;
        public ulong StripeCount;
        public MetadataStripe[] Stripes;

        internal void Parse(string head, TextReader data)
        {
            Name = head.Trim().TrimEnd('{').TrimEnd();
            string line;
            while ((line = Metadata.ReadLine(data)) != null)
            {
                if (line == String.Empty) continue;
                if (line.Contains("="))
                {
                    var parameter = Metadata.ParseParameter(line);
                    switch (parameter.Key.Trim().ToLowerInvariant())
                    {
                        case "start_extent":
                            StartExtent = Metadata.ParseNumericValue(parameter.Value);
                            break;
                        case "extent_count":
                            ExtentCount = Metadata.ParseNumericValue(parameter.Value);
                            break;
                        case "type":
                            var value = Metadata.ParseStringValue(parameter.Value);
                            if (value == "striped")
                                Type = SegmentType.Striped;
                            break;
                        case "stripe_count":
                            StripeCount = Metadata.ParseNumericValue(parameter.Value);
                            break;
                        case "stripes":
                            if (parameter.Value.Trim() == "[")
                            {
                                Stripes = ParseStripesSection(data);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(parameter.Key, "Unexpected parameter in global metadata");
                    }
                }
                else if (line.EndsWith("}"))
                {
                    return;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(line, "unexpected input");
                }
            }
        }

        private MetadataStripe[] ParseStripesSection(TextReader data)
        {
            var result = new List<MetadataStripe>();

            string line;
            while ((line = Metadata.ReadLine(data)) != null)
            {
                if (line == String.Empty) continue;
                if (line.EndsWith("]"))
                {
                    return result.ToArray();
                }
                var pv = new MetadataStripe();
                pv.Parse(line);
                result.Add(pv);
            }
            return result.ToArray();
        }
    }

    [Flags]
    internal enum SegmentType
    {
        None,
        Striped,
    }
}
