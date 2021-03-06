﻿// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

using System;
using RGB.NET.Core;

namespace RGB.NET.Decorators.Brush
{
    /// <inheritdoc cref="AbstractUpdateAwareDecorator" />
    /// <inheritdoc cref="IBrushDecorator" />
    /// <summary>
    /// Represents a decorator which allows to flash a brush by modifying his opacity.
    /// </summary>
    public class FlashDecorator : AbstractUpdateAwareDecorator, IBrushDecorator
    {
        #region Properties & Fields

        /// <summary>
        /// Gets or sets the attack-time (in seconds) of the decorator. (default: 0.2)<br />
        /// This is close to a synthesizer envelope. (See <see href="http://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope" />  as reference)
        /// </summary>
        public double Attack { get; set; } = 0.2;

        /// <summary>
        /// Gets or sets the decay-time (in seconds) of the decorator. (default: 0)<br />
        /// This is close to a synthesizer envelope. (See <see href="http://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope" /> as reference)
        /// </summary>
        public double Decay { get; set; } = 0;

        /// <summary>
        /// Gets or sets the sustain-time (in seconds) of the decorator. (default: 0.3)<br />
        /// This is close to a synthesizer envelope. (See <see href="http://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope" /> as reference)<br />
        /// Note that this value for naming reasons represents the time NOT the level.
        /// </summary>
        public double Sustain { get; set; } = 0.3;

        /// <summary>
        /// Gets or sets the release-time (in seconds) of the decorator. (default: 0.2)<br />
        /// This is close to a synthesizer envelope. (See <see href="http://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope" /> as reference)
        /// </summary>
        public double Release { get; set; } = 0.2;

        /// <summary>
        /// Gets or sets the level to which the oppacity (percentage) should raise in the attack-cycle. (default: 1);
        /// </summary>
        public double AttackValue { get; set; } = 1;

        /// <summary>
        /// Gets or sets the level at which the oppacity (percentage) should stay in the sustain-cycle. (default: 1);
        /// </summary>
        public double SustainValue { get; set; } = 1;

        /// <summary>
        /// Gets or sets the level at which the oppacity (percentage) should stay in the pause-cycle. (default: 0);
        /// </summary>
        public double PauseValue { get; set; } = 0;

        /// <summary>
        /// Gets or sets the interval (in seconds) in which the decorator should repeat (if repetition is enabled). (default: 1)
        /// </summary>
        public double Interval { get; set; } = 1;

        /// <summary>
        /// Gets or sets the amount of repetitions the decorator should do until it's finished. Zero means infinite. (default: 0)
        /// </summary>
        public int Repetitions { get; set; } = 0;

        private ADSRPhase _currentPhase;
        private double _currentPhaseValue;
        private int _repetitionCount;

        private double _currentValue;

        #endregion

        #region Methods

        /// <inheritdoc />
        public Color ManipulateColor(Rectangle rectangle, BrushRenderTarget renderTarget, Color color) => color.SetA(_currentValue);

        /// <inheritdoc />
        protected override void Update(double deltaTime)
        {
            _currentPhaseValue -= deltaTime;

            // Using ifs instead of a switch allows to skip phases with time 0.
            // ReSharper disable InvertIf

            if (_currentPhase == ADSRPhase.Attack)
                if (_currentPhaseValue > 0)
                    _currentValue = PauseValue + (Math.Min(1, (Attack - _currentPhaseValue) / Attack) * (AttackValue - PauseValue));
                else
                {
                    _currentPhaseValue = Decay;
                    _currentPhase = ADSRPhase.Decay;
                }

            if (_currentPhase == ADSRPhase.Decay)
                if (_currentPhaseValue > 0)
                    _currentValue = SustainValue + (Math.Min(1, _currentPhaseValue / Decay) * (AttackValue - SustainValue));
                else
                {
                    _currentPhaseValue = Sustain;
                    _currentPhase = ADSRPhase.Sustain;
                }

            if (_currentPhase == ADSRPhase.Sustain)
                if (_currentPhaseValue > 0)
                    _currentValue = SustainValue;
                else
                {
                    _currentPhaseValue = Release;
                    _currentPhase = ADSRPhase.Release;
                }

            if (_currentPhase == ADSRPhase.Release)
                if (_currentPhaseValue > 0)
                    _currentValue = PauseValue + (Math.Min(1, _currentPhaseValue / Release) * (SustainValue - PauseValue));
                else
                {
                    _currentPhaseValue = Interval;
                    _currentPhase = ADSRPhase.Pause;
                }

            if (_currentPhase == ADSRPhase.Pause)
                if (_currentPhaseValue > 0)
                    _currentValue = PauseValue;
                else
                {
                    if ((++_repetitionCount >= Repetitions) && (Repetitions > 0))
                        Detach<IBrush, FlashDecorator>();
                    _currentPhaseValue = Attack;
                    _currentPhase = ADSRPhase.Attack;
                }

            // ReSharper restore InvertIf
        }

        /// <inheritdoc cref="AbstractUpdateAwareDecorator.OnAttached" />
        /// <inheritdoc cref="IDecorator.OnAttached" />
        public override void OnAttached(IDecoratable decoratable)
        {
            base.OnAttached(decoratable);

            _currentPhase = ADSRPhase.Attack;
            _currentPhaseValue = Attack;
            _repetitionCount = 0;
            _currentValue = 0;
        }

        #endregion

        // ReSharper disable once InconsistentNaming
        private enum ADSRPhase
        {
            Attack,
            Decay,
            Sustain,
            Release,
            Pause
        }
    }
}
