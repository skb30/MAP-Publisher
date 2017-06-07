using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PublishResults
{
    class Product
    {
        string productID;
        string productName;

        //public void Product()
        //{

        //}

        public void addProduct (string productID, string productName) {

            this.productID = productID;
            this.productName = productName;

        }
        public string  getProduct()
        {
            return this.productName;
        }
    }

}
