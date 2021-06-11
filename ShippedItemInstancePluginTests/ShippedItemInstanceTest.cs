using AgGateway.ADAPT.ApplicationDataModel.ADM;
using System;
using System.Collections.Generic;
using System.IO;
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
                    AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin shippedItemInstancePlugin = new AgGateway.ADAPT.ShippedItemInstancePlugin.Plugin();

                    // use Path.Combine to ensure proper OS directory separator chars are applied 
                    string sampleDataPath = Path.Combine("..", "..", "..", "..", "SampleData");
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
        public void Products()
        {
            Assert.Equal(6, _testData.Models[0].Catalog.Products.Count);
            Assert.Equal(6, _testData.Models[0].Catalog.PackagedProducts.Count);
            Assert.Equal(6, _testData.Models[0].Catalog.PackagedProductInstances.Count);

            Assert.Equal(10, _testData.Models[1].Catalog.Products.Count);
            Assert.Equal(10, _testData.Models[1].Catalog.PackagedProducts.Count);
            Assert.Equal(10, _testData.Models[1].Catalog.PackagedProductInstances.Count);

            Assert.Equal(3, _testData.Models[2].Catalog.Products.Count);
            Assert.Equal(3, _testData.Models[2].Catalog.PackagedProducts.Count);
            Assert.Equal(3, _testData.Models[2].Catalog.PackagedProductInstances.Count);
        }

        [Fact]
        public void Brands()
        {
            Assert.Equal(1, _testData.Models[0].Catalog.Brands.Count);
            Assert.Equal(1, _testData.Models[1].Catalog.Brands.Count);
            Assert.Equal(1, _testData.Models[2].Catalog.Brands.Count);
        }

        [Fact]
        public void Growers()
        {
            Assert.Equal(1, _testData.Models[0].Catalog.Growers.Count);
            Assert.Equal(1, _testData.Models[1].Catalog.Growers.Count);
            Assert.Equal(1, _testData.Models[2].Catalog.Growers.Count);
        }

        [Fact]
        public void Crops()
        {
            Assert.Equal(1, _testData.Models[0].Catalog.Crops.Count);
            Assert.Equal(1, _testData.Models[1].Catalog.Crops.Count);
            Assert.Equal(1, _testData.Models[2].Catalog.Crops.Count);
        }
    }
}
