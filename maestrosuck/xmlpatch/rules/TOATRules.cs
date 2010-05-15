using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace XmlPatch.PatchRules
{
    public class CategoryRule : PatchRule
    {
        public override void Patch(XmlElement directive, ref XmlDocument doc)
        {
            string cats = directive.InnerText;

            List<XmlElement> elements = new List<XmlElement>();
            foreach (char c in cats)
            {
                foreach (XmlElement el in doc.SelectNodes("/Root/item[tag/text() = '" + c + "']"))
                {
                    elements.Add((XmlElement)el.CloneNode(true));
                }
            }
            XmlNode root = doc.DocumentElement;
            root.RemoveAll();
            foreach (XmlElement el in elements)
            {
                root.AppendChild(el);
            }
        }
    }

    public class PrependRule : XPathSelectPatchRule
    {
        protected override void PatchNodes(XmlElement directive, XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                foreach (XmlNode newNode in directive.ChildNodes)
                {
                    XmlNode clone = node.OwnerDocument.ImportNode(newNode, true);
                    node.ParentNode.InsertBefore(clone, node);
                }
            }
        }
    }

    public class OutputAuctionDisplayRule : PatchRule
    {
        public override void Patch(XmlElement directive, ref XmlDocument doc)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            XmlWriter writer = XmlWriter.Create(directive.InnerText, settings);

            writer.WriteStartElement("Root");
            writer.WriteWhitespace("\n");

            foreach (XmlElement el in doc.SelectNodes("/Root/item"))
            {
                writer.WriteStartElement("image");
                writer.WriteAttributeString("href", el.SelectSingleNode("image/@href").Value);
                writer.WriteEndElement();
                writer.WriteWhitespace("\n");

                WriteLn(writer, "name", el);
                WriteLn(writer, "desc", el);
                writer.WriteElementString("value_label", "Value: ");
                WriteLn(writer, "value", el);
                writer.WriteElementString("donor_label", "Donated by: ");
                WriteLn(writer, "donor", el);
                //WriteLn(writer, "id", el);
            }

            writer.WriteEndElement();
            writer.Close();
        }

        private void WriteLn(XmlWriter writer, string name, XmlElement el)
        {
            writer.WriteElementString(name, el.SelectSingleNode(name).InnerText);
            writer.WriteWhitespace("\n");
        }
    }

    public class OutputBookletRule : PatchRule
    {
        public override void Patch(XmlElement directive, ref XmlDocument doc)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = false;
            XmlWriter writer = XmlWriter.Create(directive.InnerText, settings);

            writer.WriteStartElement("Root");

            foreach (XmlElement el in doc.SelectNodes("/Root/item"))
            {
                string tag = el.SelectSingleNode("tag/text()").Value;
                string prefix = tag == "L" ? "live_" : "";

                /*
                writer.WriteStartElement("image");
                writer.WriteAttributeString("href", el.SelectSingleNode("image/@href").Value);
                writer.WriteEndElement();
                writer.WriteWhitespace("\n");
                 */

                WriteLn(writer, "name", el, prefix);
                WriteLn(writer, "desc", el, prefix);
                WriteLn(writer, "donor", el, prefix);
                writer.WriteElementString(prefix + "value", "Value: " + el.SelectSingleNode("value/text()").Value);
                writer.WriteWhitespace("\n");
            }

            writer.WriteEndElement();
            writer.Close();
        }

        private void WriteLn(XmlWriter writer, string name, XmlElement el, string prefix)
        {
            writer.WriteElementString(prefix + name, el.SelectSingleNode(name).InnerText);
            writer.WriteWhitespace("\n");
        }
    }
}
