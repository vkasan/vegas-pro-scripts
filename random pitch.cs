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

class EntryPoint
{
    private int audioTracksNum;
    private Dictionary<string, int> projAudioTracks = new Dictionary<string, int>();

    Vegas vegas;

    public void FromVegas(Vegas instance)
    {
        vegas = instance;

        IndexProjectTracks();

        if (audioTracksNum == 0)
        {
            MessageBox.Show("Project doesn't contain audio tracks", "FUCKERY", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

         Options? options = Prompt.GetOptions(projAudioTracks);
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
            if (track.IsAudio())
            {
                audioTracksNum++;
                projAudioTracks.Add("Track #" + (track.Index + 1).ToString() +
                                          (String.IsNullOrWhiteSpace(track.Name) ?
                                          ("") : (" | (" + track.Name + ")")), track.Index);
            }
        }
    }

    public void FlipAndDisable(Options options)
    {
        Track worktrack = vegas.Project.Tracks[options.targetTrack];

        //bool flipping = false;
        int flippedElements = 0;
        //int counter = 0;
        Random rand = new Random();
        foreach (TrackEvent ev in worktrack.Events)
        {
            AudioEvent ve = (AudioEvent)ev;
            ve.PitchSemis = rand.Next(-24,24);
            //ve.PitchSemis = -1000;
            flippedElements = flippedElements + 1;
        };

        MessageBox.Show(flippedElements == 0 ? "There was nothing to fuck" : (string.Format("{1} elements on Track #{0} have been successfully fucked", (options.targetTrack + 1), flippedElements)), "All fucked", MessageBoxButtons.OK, MessageBoxIcon.Information);

        return;
    }
}

//POHUI
class Prompt
{
    public static
    Options? GetOptions(Dictionary<string,int> videoTracks)
    {

        Form prompt = new Form()
        {
            Width = 300,
            Height = 120,
            FormBorderStyle = FormBorderStyle.FixedSingle,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowIcon = false,
            Text = "vkasan's automatic audio pitch randomizer"
        };

        Label
        instructions = new Label()
        {
            Left = 5,
            Top = 5,
            Text = "Select the audio track",
            Width = 300,
            Height = 50,
            BackColor = System.Drawing.Color.Transparent
        };
        ComboBox
        inputValue = new ComboBox()
        {
            Left = 7,
            Top = 20,
            Width = (prompt.Width - 30)
        };
        Button
        confirmation = new Button()
        {
            Text = "Fuck that shit",
            Left = 7,
            Width = 100,
            Top = 50,
            DialogResult = DialogResult.OK
        };
        inputValue.BeginUpdate();
        inputValue.DataSource = new BindingSource(videoTracks, null);
        inputValue.DisplayMember = "Key";
        inputValue.ValueMember = "Value";
        inputValue.EndUpdate();

        inputValue.DropDownStyle = ComboBoxStyle.DropDownList;

        prompt.Controls.Add(inputValue);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(instructions);
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
            };
        }
        else
        {
            return null;
        }
    }
}
