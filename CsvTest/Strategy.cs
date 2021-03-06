﻿using CsvHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;

namespace ProcessPriceActionData
{
    static class Constants
    {
        public const int TickCount = 2000; // 2000 ticks per bar
        public const int barsLookAhear = 5; // look ahead 5 bars
        public const int minBarRecords = 50; //anything less will be meaningless
    }

    class Strategy
    {
        static public void buildBarRecords(IEnumerable records, List<BarRecord> barRecords)
        {
            BarRecord bar = new BarRecord();
            foreach (DataRecord record in records)
            {
                bar.DATE = record.Date;
                bar.TIME = record.Time;
                bar.OPEN_PRICE = record.Open;
                bar.HIGH_PRICE = record.High;
                bar.LOW_PRICE = record.Low;
                bar.CLOSE_PRICE = record.Close;
                bar.TOTAL_VOLUME = record.Volume;
                barRecords.Add(bar);
                bar = new BarRecord();
            }
        }

        static public void buildIndicators(List<BarRecord> barRecords)
        {
            SMA sma9 = new SMA(9);
            sma9.LoadOhlcList(barRecords);
            SingleDoubleSerie sma9Serie = sma9.Calculate();
            double?[] sma9Arry = sma9Serie.Values.ToArray();

            SMA sma20 = new SMA(20);
            sma20.LoadOhlcList(barRecords);
            SingleDoubleSerie sma20Serie = sma20.Calculate();
            double?[] sma20Arry = sma20Serie.Values.ToArray();

            SMA sma50 = new SMA(50);
            sma50.LoadOhlcList(barRecords);
            SingleDoubleSerie sma50Serie = sma50.Calculate();
            double?[] sma50Arry = sma50Serie.Values.ToArray();

            MACD macd = new MACD(true);
            macd.LoadOhlcList(barRecords);
            MACDSerie macdSerie = macd.Calculate();
            double?[] macdHistArry = macdSerie.MACDHistogram.ToArray();

            RSI rsi = new RSI(14);
            rsi.LoadOhlcList(barRecords);
            RSISerie rsiSerie = rsi.Calculate();
            double?[] rsiArry = rsiSerie.RSI.ToArray();

            BollingerBand bollingerBand = new BollingerBand();
            bollingerBand.LoadOhlcList(barRecords);
            BollingerBandSerie bollingerSerie = bollingerBand.Calculate();
            double?[] bollLowArry = bollingerSerie.LowerBand.ToArray();
            double?[] bollUpArry = bollingerSerie.UpperBand.ToArray();

            CCI cci = new CCI();
            cci.LoadOhlcList(barRecords);
            SingleDoubleSerie cciSerie = cci.Calculate();
            double?[] cciArry = cciSerie.Values.ToArray();

            ATR atr = new ATR();
            atr.LoadOhlcList(barRecords);
            ATRSerie atrSerie = atr.Calculate();
            double?[] atrHighArry = atrSerie.TrueHigh.ToArray();
            double?[] atrLowArry = atrSerie.TrueLow.ToArray();

            ADX adx = new ADX();
            adx.LoadOhlcList(barRecords);
            ADXSerie adxSerie = adx.Calculate();
            double?[] adxPositiveArry = adxSerie.DIPositive.ToArray();
            double?[] adxNegativeArry = adxSerie.DINegative.ToArray();

            Momentum mo = new Momentum();
            mo.LoadOhlcList(barRecords);
            SingleDoubleSerie moSerie = mo.Calculate();
            double?[] moArry = moSerie.Values.ToArray();

            int index = 0;
            foreach (BarRecord bar in barRecords)
            {
                bar.SMA9 = sma9Arry[index].ToString();
                bar.SMA20 = sma20Arry[index].ToString();
                bar.SMA50 = sma50Arry[index].ToString();
                bar.MACD_DIFF = macdHistArry[index].ToString();
                bar.RSI = rsiArry[index].ToString();
                bar.BOLL_LOW = bollLowArry[index].ToString();
                bar.BOLL_HIGH = bollUpArry[index].ToString();
                bar.CCI = cciArry[index].ToString();
                bar.ADX_DIPositive = adxPositiveArry[index].ToString();
                bar.ADX_DINegative = adxNegativeArry[index].ToString();
                bar.ATR_TrueHigh = atrHighArry[index].ToString();
                bar.ATR_TrueLow = atrLowArry[index].ToString();
                bar.Momemtum = moArry[index].ToString();
                index++;
            }

        }

        // pad the indicator values that are "" with known values
        static public void padIndicators(List<BarRecord> barRecords)
        {
            barRecords.Reverse();
            double lastSMA9 = Convert.ToDouble(barRecords.First().SMA9);
            double lastSM20 = Convert.ToDouble(barRecords.First().SMA20);
            double lastSM50 = Convert.ToDouble(barRecords.First().SMA50);
            double lastMACD = Convert.ToDouble(barRecords.First().MACD_DIFF);
            double lastRSI = Convert.ToDouble(barRecords.First().RSI);
            double lastBollLow = Convert.ToDouble(barRecords.First().BOLL_LOW);
            double lastBollHigh = Convert.ToDouble(barRecords.First().BOLL_HIGH);
            double lastCCI = Convert.ToDouble(barRecords.First().CCI);
            double lastATRHigh = Convert.ToDouble(barRecords.First().ATR_TrueHigh);
            double lastATRLow = Convert.ToDouble(barRecords.First().ATR_TrueLow);
            double lastADXPositive = Convert.ToDouble(barRecords.First().ADX_DIPositive);
            double lastADXNegative = Convert.ToDouble(barRecords.First().ADX_DINegative);
            double lastMomentum = Convert.ToDouble(barRecords.First().Momemtum);

            foreach (BarRecord bar in barRecords)
            {
                if (string.IsNullOrEmpty(bar.SMA9))
                    bar.SMA9 = lastSMA9.ToString();
                else
                    lastSMA9 = Convert.ToDouble(bar.SMA9);

                if (string.IsNullOrEmpty(bar.SMA20))
                    bar.SMA20 = lastSM20.ToString();
                else
                    lastSM20 = Convert.ToDouble(bar.SMA20);

                if (string.IsNullOrEmpty(bar.SMA50))
                    bar.SMA50 = lastSM50.ToString();
                else
                    lastSM50 = Convert.ToDouble(bar.SMA50);

                if (string.IsNullOrEmpty(bar.MACD_DIFF))
                    bar.MACD_DIFF = lastMACD.ToString();
                else
                    lastMACD = Convert.ToDouble(bar.MACD_DIFF);

                if (string.IsNullOrEmpty(bar.RSI))
                    bar.RSI = lastRSI.ToString();
                else
                    lastRSI = Convert.ToDouble(bar.RSI);

                if (string.IsNullOrEmpty(bar.BOLL_HIGH))
                    bar.BOLL_HIGH = lastBollHigh.ToString();
                else
                    lastBollHigh = Convert.ToDouble(bar.BOLL_HIGH);

                if (string.IsNullOrEmpty(bar.BOLL_LOW))
                    bar.BOLL_LOW = lastBollLow.ToString();
                else
                    lastBollLow = Convert.ToDouble(bar.BOLL_LOW);

                if (string.IsNullOrEmpty(bar.CCI))
                    bar.CCI = lastCCI.ToString();
                else
                    lastCCI = Convert.ToDouble(bar.CCI);

                if (string.IsNullOrEmpty(bar.ATR_TrueHigh))
                    bar.ATR_TrueHigh = lastATRHigh.ToString();
                else
                    lastATRHigh = Convert.ToDouble(bar.ATR_TrueHigh);

                if (string.IsNullOrEmpty(bar.ATR_TrueLow))
                    bar.ATR_TrueLow = lastATRLow.ToString();
                else
                    lastATRLow = Convert.ToDouble(bar.ATR_TrueLow);

                if (string.IsNullOrEmpty(bar.ADX_DIPositive))
                    bar.ADX_DIPositive = lastADXPositive.ToString();
                else
                    lastADXPositive = Convert.ToDouble(bar.ADX_DIPositive);

                if (string.IsNullOrEmpty(bar.ADX_DINegative))
                    bar.ADX_DINegative = lastADXNegative.ToString();
                else
                    lastADXNegative = Convert.ToDouble(bar.ADX_DINegative);

                if (string.IsNullOrEmpty(bar.Momemtum))
                    bar.Momemtum = lastMomentum.ToString();
                else
                    lastMomentum = Convert.ToDouble(bar.Momemtum);
            }
            barRecords.Reverse();
        }

        static public void buildLookAhead5Bars(List<BarRecord> barRecords)
        {
            BarRecord[] barRecordArry = barRecords.ToArray();
            int index = 0;
            double lastClosePrice = 0.0;
            double lastOpenPrice = 0.0;
            foreach (BarRecord bar in barRecords)
            {
                if ((index + Constants.barsLookAhear) >= barRecordArry.Length)
                {
                    bar.NEXT_CLOSE_BAR1 = lastClosePrice.ToString();
                    bar.NEXT_CLOSE_BAR2 = lastClosePrice.ToString();
                    bar.NEXT_CLOSE_BAR3 = lastClosePrice.ToString();
                    bar.NEXT_CLOSE_BAR4 = lastClosePrice.ToString();
                    bar.NEXT_CLOSE_BAR5 = lastClosePrice.ToString();
                    bar.NEXT_OPEN_BAR1 = lastOpenPrice.ToString();
                    bar.NEXT_OPEN_BAR2 = lastOpenPrice.ToString();
                    bar.NEXT_OPEN_BAR3 = lastOpenPrice.ToString();
                    bar.NEXT_OPEN_BAR4 = lastOpenPrice.ToString();
                    bar.NEXT_OPEN_BAR5 = lastOpenPrice.ToString();

                }
                else
                {
                    bar.NEXT_CLOSE_BAR1 = barRecordArry[index + 1].CLOSE_PRICE;
                    bar.NEXT_CLOSE_BAR2 = barRecordArry[index + 2].CLOSE_PRICE;
                    bar.NEXT_CLOSE_BAR3 = barRecordArry[index + 3].CLOSE_PRICE;
                    bar.NEXT_CLOSE_BAR4 = barRecordArry[index + 4].CLOSE_PRICE;
                    bar.NEXT_CLOSE_BAR5 = barRecordArry[index + 5].CLOSE_PRICE;
                    bar.NEXT_OPEN_BAR1 = barRecordArry[index + 1].OPEN_PRICE;
                    bar.NEXT_OPEN_BAR2 = barRecordArry[index + 2].OPEN_PRICE;
                    bar.NEXT_OPEN_BAR3 = barRecordArry[index + 3].OPEN_PRICE;
                    bar.NEXT_OPEN_BAR4 = barRecordArry[index + 4].OPEN_PRICE;
                    bar.NEXT_OPEN_BAR5 = barRecordArry[index + 5].OPEN_PRICE;
                    lastClosePrice = Convert.ToDouble(bar.NEXT_CLOSE_BAR5);
                    lastOpenPrice = Convert.ToDouble(bar.NEXT_OPEN_BAR5);

                }
                index++;
            }
        }

        static public List<String> SplitESFileIntoDailyDataFiles(String esFile)
        {
            List<String> dailyDataFiles = new List<string>();
            string outputFileName;

            using (var sr = new StreamReader(esFile))
            {
                var reader = new CsvReader(sr, CultureInfo.InvariantCulture);

                //CSVReader will now read the entire es file into an enumerable
                IEnumerable records = reader.GetRecords<DataRecord>().ToList();

                List<DataRecord> listDataRecords = new List<DataRecord>();

                String startDate = "";
                bool startNewRecord = true;
                foreach (DataRecord record in records)
                {
                    if (startNewRecord || record.Date.Contains(startDate))
                    {
                        if (startNewRecord)
                        {
                            listDataRecords = new List<DataRecord>();
                            startNewRecord = false;
                            startDate = record.Date;
                        }

                        listDataRecords.Add(record);
                    }
                    else
                    {
                        outputFileName = Path.GetFileNameWithoutExtension(esFile) + "-" + startDate + ".csv";
                        using (var sw = new StreamWriter(outputFileName))
                        {
                            var writer = new CsvWriter(sw, CultureInfo.InvariantCulture);
                            writer.WriteRecords(listDataRecords);
                            writer.Flush();
                        }
                        dailyDataFiles.Add(outputFileName);
                        startNewRecord = true;
                    }
                }
                // flush last record
                outputFileName = Path.GetFileNameWithoutExtension(esFile) + "-" + startDate + ".csv";
                using (var sw = new StreamWriter(outputFileName))
                {
                    var writer = new CsvWriter(sw, CultureInfo.InvariantCulture);
                    writer.WriteRecords(listDataRecords);
                    writer.Flush();
                }
                dailyDataFiles.Add(outputFileName);
            }
            return dailyDataFiles;
        }

        static void Main(string[] args)
        {
            // To check the length of  
            // Command line arguments   
            if (args.Length == 0)
            {
                Console.WriteLine("AiTrade inputfile");
                Environment.Exit(0);
            }

            foreach (string inESFile in args)
            {
                List<String> dailyDataFiles = SplitESFileIntoDailyDataFiles(inESFile);
                IEnumerable inFiles = dailyDataFiles;
                foreach (String inFile in inFiles)
                {
                    using (var sr = new StreamReader(inFile))
                    {
                        String outFile = Path.GetFileNameWithoutExtension(inFile) + "-min-bar.csv";
                        using (var sw = new StreamWriter(outFile))
                        {
                            var reader = new CsvReader(sr, CultureInfo.InvariantCulture);
                            var writer = new CsvWriter(sw, CultureInfo.InvariantCulture);

                            //CSVReader will now read the whole file into an enumerable
                            IEnumerable records = reader.GetRecords<DataRecord>().ToList();

                            //Covert ticks into bar records
                            List<BarRecord> barRecords = new List<BarRecord>();
                            buildBarRecords(records, barRecords);

                            if (barRecords.Count() < Constants.minBarRecords)
                                continue;

                            //Calculate indicators values
                            buildIndicators(barRecords);

                            //pad the unkown indicators values with known values
                            padIndicators(barRecords);

                            //provide the lookahead bars
                            buildLookAhead5Bars(barRecords);

                            //Write the entire contents of the CSV file into another
                            //Do not use WriteHeader as WriteRecords will have done that already.
                            writer.WriteRecords(barRecords);
                            writer.Flush();
                        }
                    }
                }
            }
        }
    }
}
