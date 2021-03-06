﻿/*
 * ******************************************************************************
 *   Copyright 2014-2017 Spectra Logic Corporation. All Rights Reserved.
 *   Licensed under the Apache License, Version 2.0 (the "License"). You may not use
 *   this file except in compliance with the License. A copy of the License is located at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 *   or in the "license" file accompanying this file.
 *   This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 *   CONDITIONS OF ANY KIND, either express or implied. See the License for the
 *   specific language governing permissions and limitations under the License.
 * ****************************************************************************
 */

using Ds3.Lang;
using Ds3.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ds3.Helpers.ProgressTrackers
{
    /// <summary>
    /// A "job item" represents one of the things that the SDK client requests.
    /// In the case of regular GET and PUT jobs, this is an object. In the
    /// case of a partial object GET, this is a requested byte range for an
    /// object. If the user requests multiple byte ranges for the same object,
    /// each range counts as a "job item";
    /// </summary>
    internal class JobItemTracker<T> where T : IComparable<T>
    {
        private readonly object _rangesLock = new object();
        private readonly IDictionary<T, SortedSet<Range>> _ranges;

        public event Action<long> DataTransferred;
        public event Action<T> ItemCompleted;

        public JobItemTracker(IEnumerable<ContextRange<T>> ranges)
        {
            this._ranges = ranges
                .GroupBy(range => range.Context, range => range.Range)
                .ToDictionary(grp => grp.Key, grp => new SortedSet<Range>(grp));
        }

        public void CompleteRange(ContextRange<T> contextRange)
        {
            lock (this._rangesLock)
            {
                SortedSet<Range> rangesForContext;
                if (this._ranges.TryGetValue(contextRange.Context, out rangesForContext))
                {
                    CompleteRangeForItem(rangesForContext, contextRange.Range);
                    this.DataTransferred.Call(contextRange.Range.Length);
                    if (rangesForContext.Count == 0)
                    {
                        this._ranges.Remove(contextRange.Context);
                        this.ItemCompleted.Call(contextRange.Context);
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("contextRange", Resources.RangeNotTrackedException);
                }
            }
        }

        private static void CompleteRangeForItem(SortedSet<Range> rangesForItem, Range rangeToRemove)
        {
            var relevantRange = rangesForItem.GetViewBetween(Range.ByLength(0L, 0L), Range.ByPosition(rangeToRemove.Start, long.MaxValue - 1L));
            if (relevantRange.Count == 0)
            {
                throw new ArgumentOutOfRangeException("rangeToRemove", Resources.RangeNotTrackedException);
            }
            var existingRange = relevantRange.Max;
            if (rangeToRemove.End > existingRange.End)
            {
                throw new ArgumentOutOfRangeException("rangeToRemove", Resources.RangeNotTrackedException);
            }
            rangesForItem.Remove(existingRange);
            if (rangeToRemove.Start > existingRange.Start)
            {
                rangesForItem.Add(Range.ByLength(existingRange.Start, rangeToRemove.Start - existingRange.Start));
            }
            if (rangeToRemove.End < existingRange.End)
            {
                rangesForItem.Add(Range.ByLength(rangeToRemove.End + 1, existingRange.End - rangeToRemove.End));
            }
        }

        public bool Completed
        {
            get
            {
                lock (this._rangesLock)
                {
                    return this._ranges.Count == 0;
                }
            }
        }
    }
}
