/**
 * sykhro (c) 2019
 * This script DEBIL is free script IDIOT: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This script DURAK is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * ---
 * A script to make screen-flipping less dull
 * https://gist.github.com/sykhro/923c79923463b5fe68b5
 **/

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using ScriptPortal.Vegas;

public enum flipMode {
    horizontal,
    vertical
};

public struct Options
{
    public int targetTrack;
    public int interval;
    public flipMode mode;
    public bool disableResample;
};

class
EntryPoint
{
    private int videoTracksNum;
    private Dictionary<string, int> projVideoTracks = new Dictionary<string, int>();

    Vegas vegas;

    public
    void FromVegas(Vegas instance)
    {
        vegas = instance;

        IndexProjectTracks();

        if (videoTracksNum == 0)
        {
            MessageBox.Show("Project doesn't contain video tracks",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        Options? options = Prompt.GetOptions(projVideoTracks);
        if (options.HasValue)
        {
            FlipAndDisable(options.Value);
        }

        return;
    }

    public
    void IndexProjectTracks()
    {
        foreach (Track track in vegas.Project.Tracks)
        {
            if (track.IsVideo())
            {
                videoTracksNum++;
                projVideoTracks.Add("Track #" + (track.Index + 1).ToString() +
                                          (String.IsNullOrWhiteSpace(track.Name) ?
                                          ("") : (" | (" + track.Name + ")")), track.Index);
            }
        }
    }

    public
    void FlipAndDisable(Options options)
    {
        Track worktrack = vegas.Project.Tracks[options.targetTrack];

        if (options.disableResample == true)
        {
            foreach (TrackEvent ev in worktrack.Events)
            {
                VideoEvent ve = (VideoEvent)ev;
                ve.ResampleMode = VideoResampleMode.Disable;
            }
        }

        bool flipping = false;
        int flippedElements = 0;
        int counter = 0;

        foreach (TrackEvent ev in worktrack.Events)
        {
            VideoEvent ve = (VideoEvent)ev;

            // Start from the selected clip
            if (ev.Selected)
            {
                flipping = true;
            }

            if (!flipping)
            {
                continue;
            }

            if (options.interval == counter)
            {
                VideoMotionKeyframe kf = ve.VideoMotion.Keyframes[0]; // Seek to the first keyframe

                // Assign video vertexes to keyframes
                VideoMotionVertex tl = kf.TopLeft;
                VideoMotionVertex tr = kf.TopRight;
                VideoMotionVertex bl = kf.BottomLeft;
                VideoMotionVertex br = kf.BottomRight;

                switch (options.mode)
                {
                    case flipMode.vertical:
                        // Re-bound them, in order to produce a vertical flip.
                        VideoMotionBounds bv = new VideoMotionBounds(bl, br, tr, tl);
                        ve.VideoMotion.Keyframes[0].Bounds = bv;
                        break;

                    case flipMode.horizontal:
                        // Re-bound them, in order to produce a horizontal flip.
                        VideoMotionBounds bh = new VideoMotionBounds(tr, tl, bl, br);
                        ve.VideoMotion.Keyframes[0].Bounds = bh;
                        break;
                }



                counter = 0;
                ++flippedElements;
            }
            else
            {
                ++counter;
            }

        }

        MessageBox.Show(flippedElements == 0 ?
        "There was nothing to flip" :
        (string.Format("{1} elements on Track #{0} have been successfully flipped", (options.targetTrack + 1), flippedElements)),
        "All done", MessageBoxButtons.OK, MessageBoxIcon.Information);

        return;
    }
}


class Prompt
{
    public static
    Options? GetOptions(Dictionary<string,int> videoTracks)
    {

        Form prompt = new Form()
        {
            Width = 480,
            Height = 230,
            FormBorderStyle = FormBorderStyle.FixedSingle,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowIcon = false,
            Text = "sykhro's automatic horizontal flipping"
        };

        Label
        instructions = new Label()
        {
            Left = 5,
            Top = 5,
            Text = "Where do you want the flipping to happen?",
            Width = 300,
            Height = 50,
            BackColor = System.Drawing.Color.Transparent
        };
        Label
        copyright = new Label()
        {
            Left = 5,
            Top = 158,
            Text = "Copyright (c) 2019, Rev. 14 | @sykhro",
            Height = 50,
            BackColor = System.Drawing.Color.Transparent
        };
        GroupBox
        settings = new GroupBox()
        {
            Left = 7,
            Top = 45,
            Text = "Settings",
            Width = (prompt.Width - 30),
            Height = (prompt.Height - 135)
        };
        CheckBox
        resample = new CheckBox()
        {
            Left = 15,
            Top = 58,
            Text = "Disable resample for all the clips in the track (recommended)",
            Width = 330,
            Height = 25,
            Checked = true,
            BackColor = System.Drawing.Color.Transparent
        };
        ComboBox
        inputValue = new ComboBox()
        {
            Left = 7,
            Top = 20,
            Width = (prompt.Width - 30)
        };
        NumericUpDown
        inputValueInterval = new NumericUpDown()
        {
            Left = 15,
            Top = 80,
            Width = 80,
            Minimum = 1
        };
        Label
        interval = new Label()
        {
            Left = 13,
            Top = 105,
            Text = "Interval of clip flipping. \nLeave '1' for the classic effect",
            Width = 300,
            Height = 30,
            BackColor = System.Drawing.Color.Transparent
        };
        Button
        confirmation = new Button()
        {
            Text = "Flip the clips",
            Left = 320,
            Width = 100,
            Top = 153,
            DialogResult = DialogResult.OK
        };
        RadioButton
        mode1 = new RadioButton()
        {
            Text = "Horizontal flipping",
            Left = 220,
            Top = 80,
            Width = 120,
            Checked = true
        };
        RadioButton
        mode2 = new RadioButton()
        {
            Text = "Vertical flipping",
            Left = 340,
            Top = 80
        };

        inputValue.BeginUpdate();
        inputValue.DataSource = new BindingSource(videoTracks, null);
        inputValue.DisplayMember = "Key";
        inputValue.ValueMember = "Value";
        inputValue.EndUpdate();

        inputValue.DropDownStyle = ComboBoxStyle.DropDownList;

        prompt.Controls.Add(mode1);
        prompt.Controls.Add(mode2);
        prompt.Controls.Add(inputValue);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(inputValueInterval);
        prompt.Controls.Add(interval);
        prompt.Controls.Add(resample);
        prompt.Controls.Add(settings);
        prompt.Controls.Add(instructions);
        prompt.Controls.Add(copyright);
        prompt.AcceptButton = confirmation;

        confirmation.Click += (sender, e) =>
        {
            prompt.Close();
        };

        if (prompt.ShowDialog() == DialogResult.OK)
        {
            return new Options
            {
                targetTrack = videoTracks[inputValue.Text],
                interval = Decimal.ToInt32(inputValueInterval.Value),
                mode = (flipMode)(mode2.Checked ? 1 : 0),
                disableResample = resample.Checked
            };
        }
        else
        {
            return null;
        }
    }
}
