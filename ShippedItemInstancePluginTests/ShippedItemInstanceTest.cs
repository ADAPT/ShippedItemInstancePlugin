using AgGateway.ADAPT.ApplicationDataModel.ADM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ShippedItemInstancePluginTests
{
    public class ShippedItemInstanceTestData
    {
        private IList<ApplicationDataModel> _models;
        internal IList<ApplicationDataModel> Models
        {
            get
            {
                if (_models == null)
                {
                    AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin shippedItemInstancePlugin = new 
                        AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin();

                    // use Path.Combine to ensure proper OS directory separator chars are applied 
                    string sampleDataPath = Path.Combine("..", "..", "..", "..", "SampleData/v4");
                    _models = shippedItemInstancePlugin.Import(sampleDataPath);
                }
                return _models;
            }
        }
    }

    [CollectionDefinition("ShippedItemInstanceTestData")]
    public class TestData : ICollectionFixture<ShippedItemInstanceTestData>
    {
    }

    [Collection("ShippedItemInstanceTestData")]
    public class ShippedItemInstanceTest
    {
        ShippedItemInstanceTestData _testData;
        public ShippedItemInstanceTest(ShippedItemInstanceTestData data)
        {
            _testData = data;
        }

        [Fact]
        public void Models()
        {
            // there are only two v4 files;
            Assert.Equal(2, _testData.Models.Count);
        }

        [Fact]
        public void Products()
        {

            Assert.Equal(14, _testData.Models[0].Catalog.Products.Count);
            Assert.Single(_testData.Models[1].Catalog.Products);

            // Assert.Equal(1, _testData.Models[1].Catalog.Products.Count);
            // there are only two v4 files (vs 3) and the product count is not the same
            // Assert.Equal(3, _testData.Models[2].Catalog.Products.Count);
        }

        [Fact]
        public void Brands()
        {
            // there are more than one Brand in the first file
            // Assert.Single(_testData.Models[0].Catalog.Brands);
            Assert.Equal(2, _testData.Models[0].Catalog.Brands.Count);
            Assert.Single(_testData.Models[1].Catalog.Brands);
            // Assert.Single(_testData.Models[2].Catalog.Brands);
        }

        [Fact]
        public void Growers()
        {
            Assert.Single(_testData.Models[0].Catalog.Growers);
            Assert.Single(_testData.Models[1].Catalog.Growers);
            // Assert.Single(_testData.Models[2].Catalog.Growers);
        }

        [Fact]
        public void Crops()
        {
            // Assert.Single(_testData.Models[0].Catalog.Crops);
            Assert.Equal(2, _testData.Models[0].Catalog.Crops.Count);
            Assert.Single(_testData.Models[1].Catalog.Crops);
            //  Assert.Single(_testData.Models[2].Catalog.Crops);
        }
    }
}
