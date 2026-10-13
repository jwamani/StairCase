using System;

namespace StairCase
{
    public class StairGeometry
    {
        public StairGeometry(StairParams p)
        {
            Riser = p.Riser;
            Tread = p.Tread;
            Waist = p.Waist;
            NumRisers = p.NumRisers;
            NumTreads = p.NumRisers - 1;

            StepLength = Math.Sqrt((Tread * Tread) + (Riser * Riser));

            SoffitDrop = Waist * StepLength / Tread;

            // Fixed datums (do not change with params)
            FlightStart = StairConstants.FlightStart;
            LowerLandingEndX = StairConstants.LowerLandingLength;

            // Dynamic flight + landing
            FlightEndX = FlightStart + (NumTreads * p.Tread);
            LandingEndX = FlightEndX + p.LandingLength;
            BeamStartX = LandingEndX - p.BeamWidth;

            LandingTopY = NumRisers * p.Riser;
            LandingSoffitY = LandingTopY - p.LandingThickness;
            BeamBottomY = LandingTopY - p.BeamDepth;

            BottomStartX = FlightStart - 100; // fixed datum at bottom
            ThickeningBottomY = -p.LowerThickness - StairConstants.ThickeningDrop;
            ThickeningFlatStartX = BottomStartX + p.Chamfer;
            ThickeningFlatEndX = ThickeningFlatStartX + p.ThickeningWidth;
            ThickeningEndX = ThickeningFlatEndX + p.Chamfer;

            // Small rise where the soffit meets the lower top. It stands at
            // the thickening flat end, but slides right along the soffit if
            // the flat no longer reaches high enough, so the face can never
            // invert into a triangle. Always stays right of B1's leg.
            RiseX = Math.Max(ThickeningFlatEndX, SoffitX(StairConstants.SoffitRiseMin));
            RiseTopY = SoffitY(RiseX);
        }

public double Riser;
        public double Tread;
        public double Waist;
        public int NumRisers;
        public int NumTreads;

        public double StepLength;
        public double SoffitDrop;

        public double FlightStart;
        public double FlightEndX;
        public double LandingEndX;
        public double BeamStartX;

        public double LandingTopY;
        public double LandingSoffitY;
        public double BeamBottomY;

        public double BottomStartX;
        public double ThickeningBottomY;
        public double ThickeningFlatStartX;
        public double ThickeningFlatEndX;
        public double ThickeningEndX;
        public double LowerLandingEndX;

        public double RiseX;
        public double RiseTopY;

        public double StepCornerY(double x)
        {
            return (x - FlightStart) * Riser / Tread;
        }

        public double SoffitY(double x)
        {
            return StepCornerY(x) - SoffitDrop;
        }

        public double SoffitX(double y)
        {
            return FlightStart + ((y + SoffitDrop) * Tread / Riser);
        }

        public double BarY(double x, double squareOffset)
        {
            return StepCornerY(x) - (squareOffset * StepLength / Tread);
        }

        public double BarX(double y, double squareOffset)
        {
            return FlightStart + ((y + (squareOffset * StepLength / Tread)) * Tread / Riser);
        }
    }
}