//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Dotnet.Coveralls.Parsers;
//using Xunit;

//namespace Dotnet.Coveralls.Tests
//{
//    public class ChutzpahJsonParserTests
//    {
//        [Fact]
//        public void GenerateSourceFiles_NonRelativePath()
//        {
//            var fileContents = LoadFromResource("Dotnet.Coveralls.Tests.ChutzpahExample.json");

//            var results = CreateChutzpahParser().GenerateSourceFiles(fileContents, false);

//            Assert.Equal(2, results.Count);
//            Assert.Equal("D/path/to/file/file.ts", results.First().Name);
//            Assert.Equal(36, results.First().Coverage[0]);
//            Assert.Equal(10, results.Last().Coverage[5]);
//            Assert.Null(results.First().Coverage[7]);
//        }

//        [Fact]
//        public void GenerateSourceFiles_RelativePath()
//        {
//            var fileContents = LoadFromResource("Dotnet.Coveralls.Tests.ChutzpahExample.json");

//            var basePath = @"D:\path\to";
//            var results = CreateChutzpahParser(basePath).GenerateSourceFiles(fileContents, true);

//            Assert.Equal(2, results.Count);
//            Assert.Equal("/file/file.ts", results.First().Name);
//            Assert.Equal(36, results.First().Coverage[0]);
//            Assert.Equal(10, results.First().Coverage[5]);
//            Assert.Null(results.First().Coverage[7]);
//        }

//        private ChutzpahJsonParser CreateChutzpahParser(string basePath = null) => new ChutzpahJsonParser(new PathProcessor(basePath));

//        private string LoadFromResource(string embeddedResource)
//        {
//            var executingAssembly = GetType().GetTypeInfo().Assembly;
//            using (var stream = executingAssembly.GetManifestResourceStream(embeddedResource))
//            using (var reader = new StreamReader(stream))
//            {
//                return reader.ReadToEnd();
//            }
//        }
//    }
//}