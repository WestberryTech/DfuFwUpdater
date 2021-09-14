/*
 * Fork from https://github.com/twinearthsoftware/IntelHexFormatReader
 * MIT License
 * Version: V1.0
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IntelHexReader
{
    /// <summary>
    /// Logical representation of a MemoryBlock (an ordered collection of memory cells and registers).
    /// </summary>
    public class IntelHexContent
    {
        // CS & IP registers for 80x86 systems.

        /// <summary>
        /// Code Segment register (16-bit).
        /// </summary>
        public ushort CS { get; set; }

        /// <summary>
        /// Instruction Pointer register (16-bit).
        /// </summary>
        public ushort IP { get; set; }

        // EIP register for 80386 and higher CPU's.

        /// <summary>
        /// Extended Instruction Pointer register (32-bit).
        /// </summary>
        public uint EIP { get; set; }

        /// <summary>
        /// MemoryCells (address, data)
        /// </summary>
        public Dictionary<uint, byte> MemoryCells { get; set; }

        public IntelHexContent()
        {
            MemoryCells = new Dictionary<uint, byte>();
        }
    }

    public enum RecordType
    {
        Data,
        EndOfFile,
        ExtendedSegmentAddress,
        StartSegmentAddress,
        ExtendedLinearAddress,
        StartLinearAddress
    }

    /// <summary>
    /// Logical representation of an Intel HEX record (a single line in an Intel HEX file).
    /// </summary>
    public class IntelHexRecord
    {
        public RecordType RecordType { get; set; }
        public int ByteCount { get; set; }
        public int Address { get; set; }
        public byte[] Bytes { get; set; }
        public int CheckSum { get; set; }
    }

    public static class Extensions
    {
        /// <summary>
        /// Verify an assertion about an IntelHexRecord, and throw an IOException if the predicate is not true.
        /// </summary>
        /// <param name="record">The record to verify the assertion for.</param>
        /// <param name="predicate">The assertion to verify.</param>
        /// <param name="message">The message to show in the exception if the assertion fails.</param>
        public static void Assert(this IntelHexRecord record, Func<IntelHexRecord, bool> predicate, string message)
        {
            if (!predicate(record))
                throw new IOException(
                    string.Format("{0} -- record {1}!", message, record));
        }
    }

    public static class HexFileLineParser
    {
        private const string COLON = ":";

        /// <summary>
        /// Parse a line in an Intel Hex file.
        /// 
        /// A record consists of six parts:
        ///
        /// 1. Start code, one character (':').
        /// 2. Byte count, two hex digits.
        /// 3. Address, four hex digits.
        /// 4. Record type, two hex digits (00 to 05).
        /// 5. Data, a sequence of n bytes of data.
        /// 6. Checksum, two hex digits.
        /// 
        /// </summary>
        /// <param name="line">A record (line of text) to parse.</param>
        /// <returns></returns>
        public static IntelHexRecord ParseLine(string line)
        {
            if (line == null) throw new IOException("Line to parse can not be null");

            // At a minimum, a record should consist of start code (1 char), byte count (2 chars), adress (4 chars), 
            // record type (2 chars), checksum (2 chars) - only the data part can potentially be empty. This means 
            // the line should contain at least 11 characters, or should be deemed too short.
            if (line.Length < 11) throw new IOException(string.Format("Line '{0}' is too short!", line));

            // First character should be a colon.
            if (!line.StartsWith(COLON))
                throw new IOException(
                    string.Format("Illegal line start character ('{0}'!", line));

            // Parse byteCount, and then calculate and verify required record length
            var byteCount = TryParseByteCount(line.Substring(1, 2));
            var requiredRecordLength =
                1                   // colon
                + 2                 // byte count
                + 4                 // address
                + 2                 // record type
                + (2 * byteCount)   // data
                + 2;                // checksum

            if (line.Length != requiredRecordLength)
                throw new IOException(
                    string.Format("Line '{0}' does not have required record length of {1}!",
                        line, requiredRecordLength));

            // Parse address
            var address = TryParseAddress(line.Substring(3, 4));

            // Parse record type
            var recTypeVal = TryParseRecordType(line.Substring(7, 2));
            if (!Enum.IsDefined(typeof(RecordType), recTypeVal))
                throw new IOException(
                    string.Format("Invalid record type value: '{0}'!", recTypeVal));

            var recType = (RecordType)recTypeVal;

            // Parse bytes
            var bytes = TryParseBytes(line.Substring(9, 2 * byteCount));

            // Parse checksum
            var checkSum = TryParseCheckSum(line.Substring(9 + (2 * byteCount), 2));

            // Verify
            if (!VerifyChecksum(line, byteCount, checkSum))
                throw new IOException(string.Format("Checksum for line {0} is incorrect!", line));

            return new IntelHexRecord
            {
                ByteCount = byteCount,
                Address = address,
                RecordType = recType,
                Bytes = bytes,
                CheckSum = checkSum
            };
        }

        private static int TryParseByteCount(string hexByteCount)
        {
            try
            {
                return Convert.ToInt32(hexByteCount, 16);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format("Unable to extract byte count for '{0}'.", hexByteCount),
                    ex);
            }
        }

        private static int TryParseAddress(string hexAddress)
        {
            try
            {
                return Convert.ToInt32(hexAddress, 16);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format("Unable to extract address for '{0}'.", hexAddress),
                    ex);
            }
        }

        private static int TryParseRecordType(string hexRecType)
        {
            try
            {
                return Convert.ToInt32(hexRecType, 16);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format("Unable to extract record type for '{0}'.", hexRecType),
                    ex);
            }
        }

        private static byte[] TryParseBytes(string hexData)
        {
            try
            {
                var bytes = new byte[hexData.Length / 2];
                var counter = 0;
                foreach (var hexByte in Split(hexData, 2))
                    bytes[counter++] = (byte)Convert.ToInt32(hexByte, 16);
                return bytes;
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format("Unable to extract bytes for '{0}'.", hexData),
                    ex);
            }
        }

        private static int TryParseCheckSum(string hexCheckSum)
        {
            try
            {
                return Convert.ToInt32(hexCheckSum, 16);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format("Unable to extract checksum for '{0}'.", hexCheckSum),
                    ex);
            }
        }

        private static bool VerifyChecksum(string line, int byteCount, int checkSum)
        {
            var allbytes = new byte[5 + byteCount];
            var counter = 0;

            foreach (var hexByte in Split(line.Substring(1, (4 + byteCount) * 2), 2))
                allbytes[counter++] = (byte)Convert.ToInt32(hexByte, 16);

            var maskedSumBytes = allbytes.Sum(x => (ushort)x) & 0xff;
            var checkSumCalculated = (byte)(256 - maskedSumBytes);

            return checkSumCalculated == checkSum;
        }

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }

    public class HexFileReader
    {
        private IEnumerable<string> hexRecordLines;

        #region Constructors

        public HexFileReader(string fileName)
        {
            if (!File.Exists(fileName))
                throw new ArgumentException(string.Format("File {0} does not exist!", fileName));
            Initialize(File.ReadLines(fileName));
        }

        public HexFileReader(IEnumerable<string> hexFileContents)
        {
            Initialize(hexFileContents);
        }

        #endregion

        private void Initialize(IEnumerable<string> lines)
        {
            var fileContents = lines as IList<string> ?? lines.ToList();
            if (!fileContents.Any()) throw new ArgumentException("Hex file contents can not be empty!");
            hexRecordLines = fileContents;
        }

        /// <summary>
        /// Parse the currently loaded HEX file contents.
        /// </summary>
        /// <returns>A MemoryBlock representation of the HEX file.</returns>
        public IntelHexContent Parse()
        {
            return ReadHexFile(hexRecordLines);
        }

        private static IntelHexContent ReadHexFile(IEnumerable<string> hexRecordLines)
        {
            var result = new IntelHexContent();

            uint baseAddress = 0;
            bool encounteredEndOfFile = false;
            foreach (var hexRecordLine in hexRecordLines)
            {
                var hexRecord = HexFileLineParser.ParseLine(hexRecordLine);
                switch (hexRecord.RecordType)
                {
                    case RecordType.Data:
                        {
                            uint nextAddress = (uint)hexRecord.Address + baseAddress;
                            for (var i = 0; i < hexRecord.ByteCount; i++)
                            {
                                result.MemoryCells.Add((uint)(nextAddress + i), hexRecord.Bytes[i]);
                            }
                            break;
                        }
                    case RecordType.EndOfFile:
                        {
                            hexRecord.Assert(rec => rec.Address == 0, "Address should equal zero in EOF.");
                            hexRecord.Assert(rec => rec.ByteCount == 0, "Byte count should be zero in EOF.");
                            hexRecord.Assert(rec => rec.Bytes.Length == 0, "Number of bytes should be zero for EOF.");
                            hexRecord.Assert(rec => rec.CheckSum == 0xff, "Checksum should be 0xff for EOF.");
                            encounteredEndOfFile = true;
                            break;
                        }
                    case RecordType.ExtendedSegmentAddress:
                        {
                            hexRecord.Assert(rec => rec.ByteCount == 2, "Byte count should be 2.");
                            baseAddress = (uint)(hexRecord.Bytes[0] << 8 | hexRecord.Bytes[1]) << 4;
                            break;
                        }
                    case RecordType.ExtendedLinearAddress:
                        {
                            hexRecord.Assert(rec => rec.ByteCount == 2, "Byte count should be 2.");
                            baseAddress = (uint)(hexRecord.Bytes[0] << 8 | hexRecord.Bytes[1]) << 16;
                            break;
                        }
                    case RecordType.StartSegmentAddress:
                        {
                            hexRecord.Assert(rec => rec.ByteCount == 4, "Byte count should be 4.");
                            hexRecord.Assert(rec => rec.Address == 0, "Address should be zero.");
                            result.CS = (ushort)(hexRecord.Bytes[0] << 8 + hexRecord.Bytes[1]);
                            result.IP = (ushort)(hexRecord.Bytes[2] << 8 + hexRecord.Bytes[3]);
                            break;
                        }
                    case RecordType.StartLinearAddress:
                        hexRecord.Assert(rec => rec.ByteCount == 4, "Byte count should be 4.");
                        hexRecord.Assert(rec => rec.Address == 0, "Address should be zero.");
                        result.EIP =
                            (uint)(hexRecord.Bytes[0] << 24) + (uint)(hexRecord.Bytes[1] << 16)
                            + (uint)(hexRecord.Bytes[2] << 8) + hexRecord.Bytes[3];
                        break;
                }
            }
            if (!encounteredEndOfFile) throw new IOException("No EndOfFile marker found!");
            return result;
        }
    }
}
