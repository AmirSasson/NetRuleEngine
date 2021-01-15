using System.Collections.Generic;
using System.Linq;

namespace NetRuleEngineTests
{
    public class TestModel
    {
        public class CompositeInnerClass
        {
            public int NumericField { get; set; }

            public string TextField { get; set; }
        }

        public enum SomeEnum
        {
            Yes,
            No,
            Maybe
        }
        public CompositeInnerClass Composit { get; set; }
        public int NumericField { get; set; }

        public string TextField { get; set; }

        public List<int> PrimitivesCollection { get; set; }

        public Dictionary<string, object> KeyValueCollection { get; set; }

        public List<CompositeInnerClass> CompositeCollection { get; set; }
        public SomeEnum SomeEnumValue { get; set; }

        public IEnumerable<int> CaluculatedCollection => CompositeCollection.Select(c => c.NumericField);
    }
}
