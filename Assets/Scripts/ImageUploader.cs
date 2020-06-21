using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum ImageType
{
	PNG,
	JPG
}

public class ImageUploader : MonoBehaviour
{
	Texture2D imageTexture;
	string fieldName;
	string fileName = "defaultImageName";
	ImageType imageType = ImageType.PNG;
	string url;

	//Events
	UnityAction<string> OnErrorAction;
	UnityAction<string> OnCompleteAction;


	public static ImageUploader Initialize ()
	{
		return new GameObject ("ImageUploader").AddComponent <ImageUploader> ();
	}

	public ImageUploader SetUrl (string serverUrl)
	{
		this.url = serverUrl;
		return this;
	}

	public ImageUploader SetTexture (Texture2D texture)
	{
		this.imageTexture = texture;
		return this;
	}

	public ImageUploader SetFileName (string filename)
	{
		this.fileName = filename;
		return this;
	}

	public ImageUploader SetFieldName (string fieldName)
	{
		this.fieldName = fieldName;
		return this;
	}

	public ImageUploader SetType (ImageType type)
	{
		this.imageType = type;
		return this;
	}
	//events
	public ImageUploader OnError (UnityAction<string> action)
	{
		this.OnErrorAction = action;
		return this;
	}

	public ImageUploader OnComplete (UnityAction<string> action)
	{
		this.OnCompleteAction = action;
		return this;
	}

	public void Upload ()
	{
		//check/validate fields
		if (url == null)
			Debug.LogError ("Url is not assigned, use SetUrl( url ) to set it. ");
		//...other checks...
		//...

		StopAllCoroutines ();
		StartCoroutine (StartUploading ());
	}



	IEnumerator StartUploading ()
	{
		WWWForm form = new WWWForm ();
		byte[] textureBytes = null;

		//Get a copy of the texture, because we can't access original texure data directly. 
		Texture2D imageTexture_copy = GetTextureCopy (imageTexture);

		switch (imageType) {
			case ImageType.PNG:
				textureBytes = imageTexture_copy.EncodeToPNG ();
				break;
			case ImageType.JPG:
				textureBytes = imageTexture_copy.EncodeToJPG ();
				break;
		}

		//image file extension
		string extension = imageType.ToString ().ToLower ();

		form.AddBinaryData (fieldName, textureBytes, fileName + "." + extension, "image/" + extension);

		WWW w = new WWW (url, form);

		yield return w;

		if (w.error != null) {
			//error : 
			if (OnErrorAction != null)
				OnErrorAction (w.error); //or OnErrorAction.Invoke (w.error);
		} else {
			//success
			if (OnCompleteAction != null)
				OnCompleteAction (w.text); //or OnCompleteAction.Invoke (w.error);
		}
		w.Dispose ();
		Destroy (this.gameObject);
	}

	Texture2D GetTextureCopy (Texture2D source)
	{
		//Create a RenderTexture
		RenderTexture rt = RenderTexture.GetTemporary (
			                   source.width,
			                   source.height,
			                   0,
			                   RenderTextureFormat.Default,
			                   RenderTextureReadWrite.Linear
		                   );

		//Copy source texture to the new render (RenderTexture) 
		Graphics.Blit (source, rt);

		//Store the active RenderTexture & activate new created one (rt)
		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = rt;

		//Create new Texture2D and fill its pixels from rt and apply changes.
		Texture2D readableTexture = new Texture2D (source.width, source.height);
		readableTexture.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
		readableTexture.Apply ();

		//activate the (previous) RenderTexture and release texture created with (GetTemporary( ) ..)
		RenderTexture.active = previous;
		RenderTexture.ReleaseTemporary (rt);

		return readableTexture;
	}
}
