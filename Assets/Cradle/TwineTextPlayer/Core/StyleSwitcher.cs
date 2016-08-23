using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using Cradle;

public class StyleSwitcher : MonoBehaviour {

	public Button Link_Normal;
	public Button Link_Italic;
	public Text Word_Normal;
	public Text Word_Italic;

	void Twine_BeforeDisplayOutput(StoryOutput output) {
		var textPlayer = this.GetComponent<TwineTextPlayer>();

		if (output.Style.SettingNames.Contains("italic")) {
			textPlayer.LinkTemplate = Link_Italic;
			textPlayer.WordTemplate = Word_Italic;
		}
		else {
			textPlayer.LinkTemplate = Link_Normal;
			textPlayer.WordTemplate = Word_Normal;
		}
	}
}
