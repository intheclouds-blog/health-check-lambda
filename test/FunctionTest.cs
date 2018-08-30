using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using InTheClouds.Lambda.HealthCheck;

namespace InTheClouds.Lambda.HealthCheck.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void Function_FunctionHandler_Can_Be_Constructed()
        {
            // Act
            var function = new Function();

            // Assert
            Assert.NotNull(function);
        }

        [Fact]
        public void Function_FunctionHandler_Converts_Input_To_Upper_Case()
        {
            // Arrange
            var function = new Function();
            var context = new TestLambdaContext();
         
            // Act
            var result = function.FunctionHandler("hello world", context);

            // Assert
            Assert.Equal("HELLO WORLD", result);
        }
    }
}
