using System;

using Microsoft.WindowsAzure.Storage.Table;


namespace DictionaryEntityDemo
{
    public class Customer : TableEntity
    {
        public string Name { get; set; }
    }
}
