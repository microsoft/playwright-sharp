using System;
using System.Collections.Generic;
using System.Text;

namespace PlaywrightSharp.Tests
{
    /// <summary>
    /// Enables decorating test facts with information about their origin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PlaywrightTestAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of the attribute.
        /// </summary>
        /// <param name="fileName"><see cref="FileName"/></param>
        /// <param name="nameOfTest"><see cref="TestName"/></param>
        public PlaywrightTestAttribute(string fileName, string nameOfTest)
        {
            FileName = fileName;
            TestName = nameOfTest;
        }

        /// <summary>
        /// Creates a new instance of the attribute.
        /// </summary>
        /// <param name="fileName"><see cref="FileName"/></param>
        /// <param name="describe"><see cref="Describe"/></param>
        /// <param name="nameOfTest"><see cref="TestName"/></param>
        public PlaywrightTestAttribute(string fileName, string describe, string nameOfTest) : this(fileName, nameOfTest)
        {
            Describe = describe;
        }

        /// <summary>
        /// The file name origin of the test.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The name of the test, the decorated code is based on.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// The describe of the test, the decorated code is based on, if one exists.
        /// </summary>
        public string Describe { get; }
    }
}