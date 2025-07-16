using System.Collections.Generic;
using System.Linq;

namespace NetRuleEngineTests
{
    public class TestModel
    {
        public int NumericField { get; set; }
        public string TextField { get; set; }
        public CompositeInnerClass Composit { get; set; }
        public IEnumerable<CompositeInnerClass> CompositeCollection { get; set; }
        public IEnumerable<int> PrimitivesCollection { get; set; }
        public Dictionary<string, object> KeyValueCollection { get; set; }
        public SomeEnum SomeEnumValue { get; set; }

        public IEnumerable<int> CaluculatedCollection => CompositeCollection?.Select(x => x.NumericField);

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
