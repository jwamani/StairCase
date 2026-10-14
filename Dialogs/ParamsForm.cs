using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace StairCase
{
    public class ParamsForm : Form
    {
        readonly StairParams _p;
        readonly TextBox[] _boxes;

        public ParamsForm(StairParams p)
        {
            _p = p;

            Text = "Stair parameters";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(400, 460);

            string[] labels =
            {
                "Riser height",
                "Tread width",
                "Number of steps (risers)",
                "Waist thickness",
                "Upper landing length",
                "Upper landing thickness",
                "Beam width",
                "Beam depth",
                "Lower landing thickness",
                "Slab thickening width",
                "Slab thickening chamfer",
                "Donut spacing (max)"
            };

            double[] values =
            {
                p.Riser,
                p.Tread,
                p.NumRisers,
                p.Waist,
                p.LandingLength,
                p.LandingThickness,
                p.BeamWidth,
                p.BeamDepth,
                p.LowerThickness,
                p.ThickeningWidth,
                p.Chamfer,
                p.DonutSpacing
            };

            _boxes = new TextBox[labels.Length];

            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(12),
                GrowStyle = TableLayoutPanelGrowStyle.AddRows
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            for (int i = 0; i < labels.Length; i++)
            {
                table.Controls.Add(new Label { Text = labels[i], AutoSize = true }, 0, i);

                _boxes[i] = new TextBox
                {
                    Text = values[i].ToString("0.###", CultureInfo.InvariantCulture),
                    Width = 100
                };

                table.Controls.Add(_boxes[i], 1, i);
            }

            var draw = new Button { Text = "Draw", Width = 100 };
            var cancel = new Button { Text = "Cancel", Width = 100, DialogResult = DialogResult.Cancel };

            draw.Click += OnDraw;

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(12)
            };

            buttons.Controls.Add(cancel);
            buttons.Controls.Add(draw);

            Controls.Add(buttons);
            Controls.Add(table);

            AcceptButton = draw;
            CancelButton = cancel;
        }

        void OnDraw(object sender, EventArgs e)
        {
            double[] v = new double[_boxes.Length];

            for (int i = 0; i < _boxes.Length; i++)
            {
                double parsed;

                if (!double.TryParse(_boxes[i].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) || parsed <= 0)
                {
                    MessageBox.Show("Every value must be a number greater than zero.");
                    return;
                }

                v[i] = parsed;
            }

            if (v[2] < 2)
            {
                MessageBox.Show("Number of risers must be at least 2.");
                return;
            }

            _p.Riser = v[0];
            _p.Tread = v[1];
            _p.NumRisers = (int)v[2];
            _p.Waist = v[3];
            _p.LandingLength = v[4];
            _p.LandingThickness = v[5];
            _p.BeamWidth = v[6];
            _p.BeamDepth = v[7];
            _p.LowerThickness = v[8];
            _p.ThickeningWidth = v[9];
            _p.Chamfer = v[10];
            _p.DonutSpacing = v[11];

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}