using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml;
using System;

[CustomEditor(typeof(Font))]
public class CustomFontBuilder : Editor {
    string[] fileExtensions = new string[3] { "xml", "fnt", "txt" };
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Update Font Data")) {
            //get the Asset Path
            string path=AssetDatabase.GetAssetPath(target);
            //get the directory path
            string directory=Path.GetDirectoryName(path);
            //check if there's xml, fnt or txt file or other supportted file that have the same name

            foreach(string ext in fileExtensions)
            {
                if (parseXML(Path.Combine(directory, target.name + "."+ext)))
                {
                    //Debug.Log("found font data! " + target.name + "." + ext);
                    break;
                }
            }
        }
        base.OnInspectorGUI();
    }
    bool parseXML(string path)
    {
        if (File.Exists(path))
        {
            TextAsset xmlSource = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xmlSource.text);
            }
            catch (Exception exception)
            {
                Debug.Log("file is not xml "+exception.Message+" "+path);
                return false;
            }
            finally
            {
                XmlNode info = xmlDoc.GetElementsByTagName("info")[0];
                XmlNode common=xmlDoc.GetElementsByTagName("common")[0];
                XmlNodeList charNodes = xmlDoc.GetElementsByTagName("char");
                int charList = charNodes.Count;
                float width=int.Parse(common.Attributes["scaleW"].Value);
                float height = int.Parse(common.Attributes["scaleH"].Value);
                Font targetFont = (Font)target;
                SerializedObject mFont = new SerializedObject(targetFont);
                mFont.FindProperty("m_FontSize").floatValue= float.Parse(info.Attributes["size"].Value);
                mFont.FindProperty("m_LineSpacing").floatValue = float.Parse(common.Attributes["base"].Value);
                mFont.ApplyModifiedProperties();
                //targetFont.lineHeight = int.Parse(info.Attributes["lineHeight"].Value);
                if (charList > 0)
                {
                    
                    CharacterInfo[] charInfos = new CharacterInfo[charList];
                    
                    //targetFont.fontNames[0] = info.Attributes["face"].Value;
                    for (int i = 0; i < charList; i++)
                    {
                        XmlNode node = charNodes.Item(i);
                        int nodeID = int.Parse(node.Attributes["id"].Value);
                        int nodeOffsetX = int.Parse(node.Attributes["xoffset"].Value);
                        int nodeOffsetY = int.Parse(node.Attributes["yoffset"].Value);
                        int nodeAdvanceX = int.Parse(node.Attributes["xadvance"].Value);
                        int nodeX = int.Parse(node.Attributes["x"].Value);
                        int nodeY = int.Parse(node.Attributes["y"].Value);
                        int nodeWidth = int.Parse(node.Attributes["width"].Value);
                        int nodeHeight = int.Parse(node.Attributes["height"].Value);
                        CharacterInfo charInfo = new CharacterInfo();
                        charInfo.index = nodeID;
                        charInfo.uvTopLeft = new Vector2(nodeX / width, 1-(nodeY) /height);
                        charInfo.uvTopRight = new Vector2((nodeX+nodeWidth) / width, 1-(nodeY) / height);
                        charInfo.uvBottomLeft = new Vector2(nodeX / width, 1-(nodeY + nodeHeight) / height);
                        charInfo.uvBottomRight = new Vector2((nodeX + nodeWidth) / width, 1-(nodeY + nodeHeight) / height);
                        charInfo.minX = nodeOffsetX;
                        charInfo.maxY = -nodeOffsetY;
                        charInfo.maxX = nodeOffsetX+nodeWidth;
                        charInfo.minY = -nodeOffsetY-nodeHeight;
                        
                        charInfo.advance = nodeAdvanceX;
                        charInfos[i]= charInfo;
                    }
                    targetFont.characterInfo = charInfos;
                    EditorUtility.SetDirty(target);

                }
                else
                {
                    //Debug.Log("the xml is not in font format "+path);
                }

            }
            return true;
        }
        else
        {
            //Debug.Log("file not found "+path);
            return false;
        }
    }
}
