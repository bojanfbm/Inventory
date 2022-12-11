using System.Collections;
using BusinessLogic.Models;
using BusinessLogic.Models.Exceptions;

namespace BusinessLogic.Utility;

public class Sgtin96Parser : ISgtinParser
{
    /// <summary>
    /// Method that tries to parse Sgtin-96 data from 24 hexadecimal digit RFID tag
    /// </summary>
    /// <param name="tag">24 hexadecimal digit (96-bit) RFID tag containing Sgtin-96 data encoded</param>
    /// <returns>Sgtin object</returns>
    public Sgtin TryParse(string tag)
    {
        long companyPrefix;
        long itemReference;
        long serialNumber;
            
        if (tag.Length != 24 || SgtinHelpers.IsHex(tag) == false)
        {
            throw new InvalidSgtin("Sgtin-96 length not 96 bits");
        }

        var sgtinByteArr = Convert.FromHexString(tag);
        
        var bitsRead = 14;
        var header = (EnumHeader)sgtinByteArr[0];
        if (header == EnumHeader.Sgtin96)
        {
            var filterAndPartitionByte = sgtinByteArr[1];
                
            ValidateFilter(filterAndPartitionByte);

            var partition = (PartitionMaskEnum)(filterAndPartitionByte & (int)FilterAndPartitionMask.Partition);

            var sgtinBitArr = SgtinHelpers.HexStringToBitArray(tag);

            var companyPrefixLength = GetCompanyPrefixLength(partition, sgtinBitArr);
                

            itemReference = GetItemReference(companyPrefixLength, sgtinBitArr);

            serialNumber = GetSerialNumber();
        }
        else
        {
            throw new InvalidSgtin($"'header' value {(int)header} not valid or not Sgtin-96");
        }

        return new Sgtin
        {
            CompanyPrefix = companyPrefix,
            ItemReference = itemReference,
            SerialNumber = serialNumber.ToString()
        };

        long GetItemReference(int companyPrefixLength, IEnumerable bitArray)
        {
            const int companyPrefixAndItemReferenceLength = 44;
            var itemReferenceLength = companyPrefixAndItemReferenceLength - companyPrefixLength;
            var itemReferenceBitArr = new BitArray(bitArray.Cast<bool>().Skip(bitsRead).Take(itemReferenceLength).ToArray());
            itemReference = itemReferenceBitArr.Reverse().ToLong();
            return itemReference;
        }

        int GetCompanyPrefixLength(PartitionMaskEnum partition, IEnumerable bitArray)
        {
            var companyPrefixLength = partition switch
            {
                PartitionMaskEnum.Value0 => 40,
                PartitionMaskEnum.Value1 => 37,
                PartitionMaskEnum.Value2 => 34,
                PartitionMaskEnum.Value3 => 30,
                PartitionMaskEnum.Value4 => 27,
                PartitionMaskEnum.Value5 => 24,
                PartitionMaskEnum.Value6 => 20,
                _ => throw new InvalidSgtin($"'partition' value {partition} not valid")
            };

            var companyPrefixBitArr = new BitArray(bitArray.Cast<bool>().Skip(bitsRead).Take(companyPrefixLength).ToArray());
            companyPrefix = companyPrefixBitArr.Reverse().ToLong();
            bitsRead += companyPrefixLength;

            return companyPrefixLength;
        }

        long GetSerialNumber()
        {
            var serialNumberBytesArray = sgtinByteArr[7..12];
            serialNumberBytesArray[0] = BitConverter.GetBytes(sgtinByteArr[0] | 63)[0];
            if (BitConverter.IsLittleEndian) Array.Reverse(serialNumberBytesArray);
            serialNumber = BitConverter.ToUInt32(serialNumberBytesArray, 0);
            return serialNumber;
        }

        void ValidateFilter(byte filterAndPartitionByte)
        {
            var filter = (EnumFilter) (filterAndPartitionByte & (int) FilterAndPartitionMask.Filter);

            if (Enum.IsDefined(filter) == false)
            {
                throw new InvalidSgtin($"'filter' value {(int) filter} not valid");
            }
        }
    }
}