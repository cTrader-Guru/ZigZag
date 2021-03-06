﻿/*  CTRADER GURU --> Template 1.0.6

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/
    TOS         : https://ctrader.guru/termini-del-servizio/

*/

using System;
using cAlgo.API;

namespace cAlgo.Indicators
{

    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class ZigZag : Indicator
    {

        #region Enums

        public enum ModeZigZag
        {

            HighLow,
            OpenClose

        }

        #endregion

        #region Identity

        /// <summary>
        /// Nome del prodotto, identificativo, da modificare con il nome della propria creazione
        /// </summary>
        public const string NAME = "ZigZag";

        /// <summary>
        /// La versione del prodotto, progressivo, utilie per controllare gli aggiornamenti se viene reso disponibile sul sito ctrader.guru
        /// </summary>
        public const string VERSION = "1.0.3";

        #endregion

        #region Params

        /// <summary>
        /// Identità del prodotto nel contesto di ctrader.guru
        /// </summary>
        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://ctrader.guru/product/zigzag/")]
        public string ProductInfo { get; set; }

        [Parameter("Mode", DefaultValue = ModeZigZag.HighLow, Group = "Params")]
        public ModeZigZag MyModeZigZag { get; set; }

        [Parameter(DefaultValue = 12, Group = "Params")]
        public int Depth { get; set; }

        [Parameter(DefaultValue = 5, Group = "Params")]
        public int Deviation { get; set; }

        [Parameter(DefaultValue = 3, Group = "Params")]
        public int BackStep { get; set; }

        [Output("ZigZag", LineColor = "DodgerBlue", LineStyle = LineStyle.Lines)]
        public IndicatorDataSeries Result { get; set; }

        #endregion

        #region Property

        private double _lastLow;
        private double _lastHigh;
        private double _low;
        private double _high;
        private int _lastHighIndex;
        private int _lastLowIndex;
        private int _type;
        private double _point;
        private double _currentLow;
        private double _currentHigh;

        private IndicatorDataSeries _highZigZags;
        private IndicatorDataSeries _lowZigZags;

        #endregion

        #region Indicator Events

        protected override void Initialize()
        {

            // --> Stampo nei log la versione corrente
            Print("{0} : {1}", NAME, VERSION);
            
            _highZigZags = CreateDataSeries();
            _lowZigZags = CreateDataSeries();
            _point = Symbol.TickSize;

        }

        public override void Calculate(int index)
        {

            switch (MyModeZigZag)
            {

                case ModeZigZag.HighLow:

                    _performIndicatorHighLow(index);

                    break;

                case ModeZigZag.OpenClose:

                    _performIndicatorOpenClose(index);

                    break;

            }

        }

        #endregion

        #region Private Methods

        private void _performIndicatorHighLow(int index)
        {

            if (index < Depth)
            {
                Result[index] = 0;
                _highZigZags[index] = 0;
                _lowZigZags[index] = 0;
                return;
            }

            _currentLow = Functions.Minimum(Bars.LowPrices, Depth);

            if (Math.Abs(_currentLow - _lastLow) < double.Epsilon)
                _currentLow = 0.0;
            else
            {

                _lastLow = _currentLow;

                if ((Bars.LowPrices[index] - _currentLow) > (Deviation * _point))
                    _currentLow = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_lowZigZags[index - i]) > double.Epsilon && _lowZigZags[index - i] > _currentLow)
                            _lowZigZags[index - i] = 0.0;
                    }
                }
            }
            if (Math.Abs(Bars.LowPrices[index] - _currentLow) < double.Epsilon)
                _lowZigZags[index] = _currentLow;
            else
                _lowZigZags[index] = 0.0;

            _currentHigh = Bars.HighPrices.Maximum(Depth);

            if (Math.Abs(_currentHigh - _lastHigh) < double.Epsilon)
                _currentHigh = 0.0;
            else
            {

                _lastHigh = _currentHigh;

                if ((_currentHigh - Bars.HighPrices[index]) > (Deviation * _point))
                    _currentHigh = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_highZigZags[index - i]) > double.Epsilon && _highZigZags[index - i] < _currentHigh)
                            _highZigZags[index - i] = 0.0;
                    }
                }
            }

            if (Math.Abs(Bars.HighPrices[index] - _currentHigh) < double.Epsilon)
                _highZigZags[index] = _currentHigh;
            else
                _highZigZags[index] = 0.0;


            switch (_type)
            {
                case 0:
                    if (Math.Abs(_low - 0) < double.Epsilon && Math.Abs(_high - 0) < double.Epsilon)
                    {
                        if (Math.Abs(_highZigZags[index]) > double.Epsilon)
                        {
                            _high = Bars.HighPrices[index];
                            _lastHighIndex = index;
                            _type = -1;
                            Result[index] = _high;
                        }
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon)
                        {
                            _low = Bars.LowPrices[index];
                            _lastLowIndex = index;
                            _type = 1;
                            Result[index] = _low;
                        }
                    }
                    break;
                case 1:
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && _lowZigZags[index] < _low && Math.Abs(_highZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastLowIndex] = double.NaN;
                        _lastLowIndex = index;
                        _low = _lowZigZags[index];
                        Result[index] = _low;
                    }
                    if (Math.Abs(_highZigZags[index] - 0.0) > double.Epsilon && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        _high = _highZigZags[index];
                        _lastHighIndex = index;
                        Result[index] = _high;
                        _type = -1;
                    }
                    break;
                case -1:
                    if (Math.Abs(_highZigZags[index]) > double.Epsilon && _highZigZags[index] > _high && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastHighIndex] = double.NaN;
                        _lastHighIndex = index;
                        _high = _highZigZags[index];
                        Result[index] = _high;
                    }
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && Math.Abs(_highZigZags[index]) < double.Epsilon)
                    {
                        _low = _lowZigZags[index];
                        _lastLowIndex = index;
                        Result[index] = _low;
                        _type = 1;
                    }
                    break;
                default:
                    return;
            }

        }

        private void _performIndicatorOpenClose(int index)
        {

            if (index < Depth)
            {
                Result[index] = 0;
                _highZigZags[index] = 0;
                _lowZigZags[index] = 0;
                return;
            }

            _currentLow = Functions.Minimum(Bars.OpenPrices, Depth);

            if (Math.Abs(_currentLow - _lastLow) < double.Epsilon)
                _currentLow = 0.0;
            else
            {

                _lastLow = _currentLow;

                if ((Bars.OpenPrices[index] - _currentLow) > (Deviation * _point))
                    _currentLow = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_lowZigZags[index - i]) > double.Epsilon && _lowZigZags[index - i] > _currentLow)
                            _lowZigZags[index - i] = 0.0;
                    }
                }
            }
            if (Math.Abs(Bars.OpenPrices[index] - _currentLow) < double.Epsilon)
                _lowZigZags[index] = _currentLow;
            else
                _lowZigZags[index] = 0.0;

            _currentHigh = Bars.OpenPrices.Maximum(Depth);

            if (Math.Abs(_currentHigh - _lastHigh) < double.Epsilon)
                _currentHigh = 0.0;
            else
            {

                _lastHigh = _currentHigh;

                if ((_currentHigh - Bars.OpenPrices[index]) > (Deviation * _point))
                    _currentHigh = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_highZigZags[index - i]) > double.Epsilon && _highZigZags[index - i] < _currentHigh)
                            _highZigZags[index - i] = 0.0;
                    }
                }
            }

            if (Math.Abs(Bars.OpenPrices[index] - _currentHigh) < double.Epsilon)
                _highZigZags[index] = _currentHigh;
            else
                _highZigZags[index] = 0.0;


            switch (_type)
            {
                case 0:
                    if (Math.Abs(_low - 0) < double.Epsilon && Math.Abs(_high - 0) < double.Epsilon)
                    {
                        if (Math.Abs(_highZigZags[index]) > double.Epsilon)
                        {
                            _high = Bars.OpenPrices[index];
                            _lastHighIndex = index;
                            _type = -1;
                            Result[index] = _high;
                        }
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon)
                        {
                            _low = Bars.OpenPrices[index];
                            _lastLowIndex = index;
                            _type = 1;
                            Result[index] = _low;
                        }
                    }
                    break;
                case 1:
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && _lowZigZags[index] < _low && Math.Abs(_highZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastLowIndex] = double.NaN;
                        _lastLowIndex = index;
                        _low = _lowZigZags[index];
                        Result[index] = _low;
                    }
                    if (Math.Abs(_highZigZags[index] - 0.0) > double.Epsilon && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        _high = _highZigZags[index];
                        _lastHighIndex = index;
                        Result[index] = _high;
                        _type = -1;
                    }
                    break;
                case -1:
                    if (Math.Abs(_highZigZags[index]) > double.Epsilon && _highZigZags[index] > _high && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastHighIndex] = double.NaN;
                        _lastHighIndex = index;
                        _high = _highZigZags[index];
                        Result[index] = _high;
                    }
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && Math.Abs(_highZigZags[index]) < double.Epsilon)
                    {
                        _low = _lowZigZags[index];
                        _lastLowIndex = index;
                        Result[index] = _low;
                        _type = 1;
                    }
                    break;
                default:
                    return;
            }

        }

        private void _performIndicatorOpenCloseOld(int index)
        {

            if (index < Depth)
            {
                Result[index] = 0;
                _highZigZags[index] = 0;
                _lowZigZags[index] = 0;
                return;
            }

            _currentLow = Functions.Minimum(Bars.OpenPrices, Depth);

            if (Math.Abs(_currentLow - _lastLow) < double.Epsilon)
                _currentLow = 0.0;
            else
            {

                _lastLow = _currentLow;

                if ((Bars.OpenPrices[index] - _currentLow) > (Deviation * _point))
                    _currentLow = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_lowZigZags[index - i]) > double.Epsilon && _lowZigZags[index - i] > _currentLow)
                            _lowZigZags[index - i] = 0.0;
                    }
                }
            }
            if (Math.Abs(Bars.OpenPrices[index] - _currentLow) < double.Epsilon)
                _lowZigZags[index] = _currentLow;
            else
                _lowZigZags[index] = 0.0;

            _currentHigh = Bars.ClosePrices.Maximum(Depth);

            if (Math.Abs(_currentHigh - _lastHigh) < double.Epsilon)
                _currentHigh = 0.0;
            else
            {

                _lastHigh = _currentHigh;

                if ((_currentHigh - Bars.ClosePrices[index]) > (Deviation * _point))
                    _currentHigh = 0.0;
                else
                {
                    for (int i = 1; i <= BackStep; i++)
                    {
                        if (Math.Abs(_highZigZags[index - i]) > double.Epsilon && _highZigZags[index - i] < _currentHigh)
                            _highZigZags[index - i] = 0.0;
                    }
                }
            }

            if (Math.Abs(Bars.ClosePrices[index] - _currentHigh) < double.Epsilon)
                _highZigZags[index] = _currentHigh;
            else
                _highZigZags[index] = 0.0;


            switch (_type)
            {
                case 0:
                    if (Math.Abs(_low - 0) < double.Epsilon && Math.Abs(_high - 0) < double.Epsilon)
                    {
                        if (Math.Abs(_highZigZags[index]) > double.Epsilon)
                        {
                            _high = Bars.ClosePrices[index];
                            _lastHighIndex = index;
                            _type = -1;
                            Result[index] = _high;
                        }
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon)
                        {
                            _low = Bars.OpenPrices[index];
                            _lastLowIndex = index;
                            _type = 1;
                            Result[index] = _low;
                        }
                    }
                    break;
                case 1:
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && _lowZigZags[index] < _low && Math.Abs(_highZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastLowIndex] = double.NaN;
                        _lastLowIndex = index;
                        _low = _lowZigZags[index];
                        Result[index] = _low;
                    }
                    if (Math.Abs(_highZigZags[index] - 0.0) > double.Epsilon && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        _high = _highZigZags[index];
                        _lastHighIndex = index;
                        Result[index] = _high;
                        _type = -1;
                    }
                    break;
                case -1:
                    if (Math.Abs(_highZigZags[index]) > double.Epsilon && _highZigZags[index] > _high && Math.Abs(_lowZigZags[index] - 0.0) < double.Epsilon)
                    {
                        Result[_lastHighIndex] = double.NaN;
                        _lastHighIndex = index;
                        _high = _highZigZags[index];
                        Result[index] = _high;
                    }
                    if (Math.Abs(_lowZigZags[index]) > double.Epsilon && Math.Abs(_highZigZags[index]) < double.Epsilon)
                    {
                        _low = _lowZigZags[index];
                        _lastLowIndex = index;
                        Result[index] = _low;
                        _type = 1;
                    }
                    break;
                default:
                    return;
            }

        }

        #endregion


    }

}
