﻿#region License

//     A simplistic Behavior Tree implementation in C#
//     Copyright (C) 2010  ApocDev apocdev@gmail.com
// 
//     This file is part of TreeSharp.
// 
//     TreeSharp is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     TreeSharp is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;

namespace TreeSharp
{
    public delegate bool CanRunDecoratorDelegate(object context);

    public class Decorator : GroupComposite
    {
        public Decorator(Composite child, CanRunDecoratorDelegate func = null) : base(child)
        {
            Runner = func;
        }

        protected CanRunDecoratorDelegate Runner { get; private set; }
        public Composite DecoratedChild { get { return Children[0]; } }

        protected virtual bool CanRun(object context)
        {
            return true;
        }

        public override void Start(object context)
        {
            if (Children.Count != 1)
                throw new ApplicationException("Decorators must have only one child.");
            base.Start(context);
        }

        public override IEnumerable<RunStatus> Execute(object context)
        {
            if (Runner != null)
            {
                if (!Runner(context))
                {
                    yield return RunStatus.Failure;
                    yield break;
                }
            }
            else if (!CanRun(context))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            DecoratedChild.Start(context);
            while (DecoratedChild.Tick(context) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }

            DecoratedChild.Stop(context);
            if (DecoratedChild.LastStatus == RunStatus.Failure)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            yield return RunStatus.Success;
            yield break;
        }
    }
}