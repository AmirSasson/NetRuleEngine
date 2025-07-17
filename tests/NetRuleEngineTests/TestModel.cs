using System;
using System.Collections.Generic;
using System.Linq;

namespace NetRuleEngineTests
{
    public class TestModel
    {
        public int NumericField { get; set; }
        public string TextField { get; set; }
        public CompositeInnerClass Composite { get; set; }
        public IEnumerable<CompositeInnerClass> CompositeCollection { get; set; }
        public IEnumerable<int> PrimitivesCollection { get; set; }
        public Dictionary<string, object> KeyValueCollection { get; set; }
        public SomeEnum SomeEnumValue { get; set; }
        public DateTime DateField { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<int> CalculatedCollection => CompositeCollection?.Select(x => x.NumericField);

        public class CompositeInnerClass
        {
            public int NumericField { get; set; }
        }

        public enum SomeEnum
        {
            Yes,
            No
        }
    }
}
