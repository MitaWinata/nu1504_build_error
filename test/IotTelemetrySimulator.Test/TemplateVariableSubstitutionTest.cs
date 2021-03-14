﻿namespace IotTelemetrySimulator.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class TemplateVariableSubstitutionTest
    {
        private class CustomClass
        {
            public override string ToString()
            {
                return "some_data";
            }
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void ShouldSubstituteVariablesCorrectly(Dictionary<string, object> vars, string template, string expectedPayload)
        {
            var variables = new TelemetryValues(
                vars.Select(var => new TelemetryVariable
                {
                    Name = var.Key,
                    Values = new[] { var.Value },
                }).ToArray());

            var telemetryTemplate = new TelemetryTemplate(
                template,
                variables.NextValues(null).Keys);

            var generatedPayload = telemetryTemplate.Create(variables.NextValues(null));

            Assert.Equal(expectedPayload, generatedPayload);
        }

        public static IEnumerable<object[]> GetTestData()
        {
            // Should convert variable values to string
            var customObj = new CustomClass();
            yield return new object[]
            {
                new Dictionary<string, object> { { "name", "World" }, { "var1", customObj }, { "var2", -0.5 }, { "var3", string.Empty } },
                "Hello, $.name! I like $.var1, $.var2 and $.var3, $.name.",
                "Hello, World! I like some_data, -0.5 and , World.",
            };

            // Should ignore extra variables
            yield return new object[]
            {
                new Dictionary<string, object> { { "var1", 1 }, { "var2", 2 } },
                "Only $.var1",
                "Only 1",
            };

            // Should ignore extra variables
            yield return new object[]
            {
                new Dictionary<string, object> { { "var1", 1 }, { "var2", 2 } },
                "Only $.var1",
                "Only 1",
            };

            // Should ignore non-existent variables in the template
            yield return new object[]
            {
                new Dictionary<string, object> { { "var1", 1 } },
                "$.var1 $.var",
                "1 $.var",
            };

            // Should substitute longer names first
            yield return new object[]
            {
                new Dictionary<string, object> { { "var1", 1 }, { "var", 2 }, { "var11", 3 } },
                "$.var1$.var11, $.var $.var$.var1$.var11!",
                "13, 2 213!",
            };
        }
    }
}
