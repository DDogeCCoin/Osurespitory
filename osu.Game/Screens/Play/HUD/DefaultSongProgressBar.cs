﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Framework.Threading;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgressBar : SliderBar<double>
    {
        /// <summary>
        /// Action which is invoked when a seek is requested, with the proposed millisecond value for the seek operation.
        /// </summary>
        public Action<double>? OnSeek { get; set; }

        /// <summary>
        /// Whether the progress bar should allow interaction, ie. to perform seek operations.
        /// </summary>
        public bool Interactive
        {
            get => showHandle;
            set
            {
                if (value == showHandle)
                    return;

                showHandle = value;

                handleBase.FadeTo(showHandle ? 1 : 0, 200);
            }
        }

        public Color4 FillColour
        {
            set => fill.Colour = value;
        }

        public double StartTime
        {
            set => CurrentNumber.MinValue = value;
        }

        public double EndTime
        {
            set => CurrentNumber.MaxValue = value;
        }

        public double CurrentTime
        {
            set => CurrentNumber.Value = value;
        }

        private readonly Box fill;
        private readonly Container handleBase;
        private readonly Container handleContainer;

        private bool showHandle;

        public DefaultSongProgressBar(float barHeight, float handleBarHeight, Vector2 handleSize)
        {
            CurrentNumber.MinValue = 0;
            CurrentNumber.MaxValue = 1;

            RelativeSizeAxes = Axes.X;
            Height = barHeight + handleBarHeight + handleSize.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Background",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = barHeight,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                    Depth = 1,
                },
                fill = new Box
                {
                    Name = "Fill",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = barHeight,
                },
                handleBase = new Container
                {
                    Name = "HandleBar container",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Width = 2,
                    Alpha = 0,
                    Colour = Color4.White,
                    Position = new Vector2(2, 0),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Name = "HandleBar box",
                            RelativeSizeAxes = Axes.Both,
                        },
                        handleContainer = new Container
                        {
                            Name = "Handle container",
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.TopCentre,
                            Size = handleSize,
                            CornerRadius = 5,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Handle box",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void UpdateValue(float value)
        {
            // handled in update
        }

        protected override void Update()
        {
            base.Update();

            handleBase.Height = Height - handleContainer.Height;
            float newX = (float)Interpolation.Lerp(handleBase.X, NormalizedValue * UsableWidth, Math.Clamp(Time.Elapsed / 40, 0, 1));

            fill.Width = newX;
            handleBase.X = newX;
        }

        private ScheduledDelegate? scheduledSeek;

        protected override void OnUserChange(double value)
        {
            scheduledSeek?.Cancel();
            scheduledSeek = Schedule(() =>
            {
                if (showHandle)
                    OnSeek?.Invoke(value);
            });
        }
    }
}
