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
            tracksNum++;
        }

        if (tracksNum == 0)
        {
            MessageBox.Show("Project doesn't contain tracks", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }
        SelectEventsOnTrack();

        return;
    }

    public void SelectEventsOnTrack()
    {
    	int flippedElements = 0;
    	foreach (Track track in vegas.Project.Tracks)
        {
        	if(track.Selected)
        	{
        		foreach (TrackEvent ev in track.Events)
		        {
		            ev.Selected = true;
		            flippedElements = flippedElements + 1;
		        };
        	}
        }

        //MessageBox.Show(flippedElements == 0 ? "There was nothing to select" : (string.Format("{1} elements on {0} track(s) have been successfully selected", tracksNum, flippedElements)), "All done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        if(flippedElements == 0)
        {
			MessageBox.Show("There was nothing to select");
        }

        return;
    }
}