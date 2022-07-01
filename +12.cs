using System;
using System.Windows.Forms;
using System.Collections.Generic;
using ScriptPortal.Vegas;

class EntryPoint
{
    private int tracksNum;

    Vegas vegas;

    public void FromVegas(Vegas instance)
    {
        vegas = instance;
        foreach (Track track in vegas.Project.Tracks)
        {
            if (track.IsAudio())
            {
                foreach (TrackEvent ev in track.Events)
                {
                    if(ev.Selected)
                    {
                        AudioEvent ve = (AudioEvent)ev;
                        ve.PitchSemis=ve.PitchSemis+12f;
                    }
                }
            }
        }
        return;
    }
}