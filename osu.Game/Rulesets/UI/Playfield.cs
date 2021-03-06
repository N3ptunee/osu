﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.UI
{
    public abstract class Playfield : ScalableContainer
    {
        /// <summary>
        /// The <see cref="DrawableHitObject"/> contained in this Playfield.
        /// </summary>
        public HitObjectContainer HitObjects { get; private set; }

        /// <summary>
        /// All the <see cref="DrawableHitObject"/>s contained in this <see cref="Playfield"/> and all <see cref="NestedPlayfields"/>.
        /// </summary>
        public IEnumerable<DrawableHitObject> AllHitObjects => HitObjects?.Objects.Concat(NestedPlayfields.SelectMany(p => p.AllHitObjects)) ?? Enumerable.Empty<DrawableHitObject>();

        /// <summary>
        /// All <see cref="Playfield"/>s nested inside this <see cref="Playfield"/>.
        /// </summary>
        public IEnumerable<Playfield> NestedPlayfields => nestedPlayfields.IsValueCreated ? nestedPlayfields.Value : Enumerable.Empty<Playfield>();

        private readonly Lazy<List<Playfield>> nestedPlayfields = new Lazy<List<Playfield>>();

        /// <summary>
        /// Whether judgements should be displayed by this and and all nested <see cref="Playfield"/>s.
        /// </summary>
        public readonly BindableBool DisplayJudgements = new BindableBool(true);

        /// <summary>
        /// A container for keeping track of DrawableHitObjects.
        /// </summary>
        /// <param name="customWidth">The width to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customHeight"/> is desired. If <paramref name="customHeight"/> is also null, no scaling will occur.
        /// </param>
        /// <param name="customHeight">The height to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customWidth"/> is desired. If <paramref name="customWidth"/> is also null, no scaling will occur.
        /// </param>
        protected Playfield(float? customWidth = null, float? customHeight = null)
            : base(customWidth, customHeight)
        {
            RelativeSizeAxes = Axes.Both;
        }

        private WorkingBeatmap beatmap;

        [BackgroundDependencyLoader]
        private void load(IBindableBeatmap beatmap)
        {
            this.beatmap = beatmap.Value;

            HitObjects = CreateHitObjectContainer();
            HitObjects.RelativeSizeAxes = Axes.Both;

            Add(HitObjects);
        }

        /// <summary>
        /// Performs post-processing tasks (if any) after all DrawableHitObjects are loaded into this Playfield.
        /// </summary>
        public virtual void PostProcess() => NestedPlayfields.ForEach(p => p.PostProcess());

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to add.</param>
        public virtual void Add(DrawableHitObject h) => HitObjects.Add(h);

        /// <summary>
        /// Remove a DrawableHitObject from this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to remove.</param>
        public virtual void Remove(DrawableHitObject h) => HitObjects.Remove(h);

        /// <summary>
        /// Registers a <see cref="Playfield"/> as a nested <see cref="Playfield"/>.
        /// This does not add the <see cref="Playfield"/> to the draw hierarchy.
        /// </summary>
        /// <param name="otherPlayfield">The <see cref="Playfield"/> to add.</param>
        protected void AddNested(Playfield otherPlayfield)
        {
            otherPlayfield.DisplayJudgements.BindTo(DisplayJudgements);
            nestedPlayfields.Value.Add(otherPlayfield);
        }

        /// <summary>
        /// Creates the container that will be used to contain the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectContainer CreateHitObjectContainer() => new HitObjectContainer();

        protected override void Update()
        {
            base.Update();

            if (beatmap != null)
                foreach (var mod in beatmap.Mods.Value)
                    if (mod is IUpdatableByPlayfield updatable)
                        updatable.Update(this);
        }
    }
}
