﻿using System;
using Rhino;
using Rhino.Commands;

namespace SampleCsCommands
{
  [System.Runtime.InteropServices.Guid("65431fb4-23de-4592-815f-71aa1ef39f9d")]
  public class SampleCsModifySphereRadius : Command
  {
    public SampleCsModifySphereRadius()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsModifySphereRadius"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
      go.SetCommandPrompt("Select sphere");
      go.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
      go.GeometryAttributeFilter = Rhino.Input.Custom.GeometryAttributeFilter.ClosedSurface;
      go.SubObjectSelect = false;
      go.Get();
      if (go.CommandResult() != Result.Success)
        return go.CommandResult();

      Rhino.Geometry.Brep brep = go.Object(0).Brep();
      if (null == brep || 1 != brep.Faces.Count)
        return Result.Failure;

      Rhino.Geometry.Surface srf = brep.Faces[0].UnderlyingSurface();
      if (null == srf)
        return Result.Failure;

      Rhino.Geometry.Sphere sphere;
      if (!srf.TryGetSphere(out sphere))
      {
        RhinoApp.WriteLine("Surface is not a sphere");
        return Result.Nothing;
      }

      Rhino.Input.Custom.GetNumber gn = new Rhino.Input.Custom.GetNumber();
      gn.SetCommandPrompt("New radius");
      gn.SetDefaultNumber(sphere.Radius);
      gn.SetLowerLimit(1.0, false); // or whatever you deem appripriate...
      gn.Get();
      if (gn.CommandResult() != Result.Success)
        return gn.CommandResult();

      sphere.Radius = gn.Number();

      // Sometimes, Surface.TryGetSphere() will return a sphere with a left-handed
      // plane. So, ensure the plane is right-handed.
      Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane(
        sphere.EquitorialPlane.Origin, 
        sphere.EquitorialPlane.XAxis, 
        sphere.EquitorialPlane.YAxis
        );

      sphere.EquitorialPlane = plane;

      Rhino.Geometry.RevSurface rev_srf = sphere.ToRevSurface();
      if (null != rev_srf)
      {
        doc.Objects.Replace(go.Object(0).ObjectId, rev_srf);
        doc.Views.Redraw();
      }

      return Result.Success;
    }
  }
}
