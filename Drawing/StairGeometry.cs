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

            FlightStart = StairConstants.FlightStart;
            LowerLandingEndX = StairConstants.LowerLandingLength;

            FlightEndX = FlightStart + (NumTreads * p.Tread);
            LandingEndX = FlightEndX + p.LandingLength;
            BeamStartX = LandingEndX - p.BeamWidth;

            LandingTopY = NumRisers * p.Riser;
            LandingSoffitY = LandingTopY - p.LandingThickness;
            BeamBottomY = LandingTopY - p.BeamDepth;

            BottomStartX = FlightStart - 100;
            ThickeningBottomY = -p.LowerThickness - StairConstants.ThickeningDrop;
            ThickeningFlatStartX = BottomStartX + p.Chamfer;
            ThickeningFlatEndX = ThickeningFlatStartX + p.ThickeningWidth;
            ThickeningEndX = ThickeningFlatEndX + p.Chamfer;

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