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
                writer.WriteAttributeString("href", ValueOrNothing(el.SelectSingleNode("image/@href")));
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

        private string ValueOrNothing(XmlNode node)
        {
            return node != null ? node.Value : "";
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

            int order = 1;
            foreach (XmlElement el in doc.SelectNodes("/Root/item"))
            {
                string tag = el.SelectSingleNode("tag/text()").Value;
                bool live = tag == "L";
                string prefix = live ? "live_" : "";

                /*
                writer.WriteStartElement("image");
                writer.WriteAttributeString("href", el.SelectSingleNode("image/@href").Value);
                writer.WriteEndElement();
                writer.WriteWhitespace("\n");
                 */

                if (live)
                {
                    writer.WriteElementString(prefix + "order", order++.ToString());
                    writer.WriteWhitespace("\n");
                }
                WriteLn(writer, "name", el, prefix);
                if (live)
                {
                    foreach (XmlElement image in el.SelectNodes("image"))
                    {
                        writer.WriteRaw(image.OuterXml);
                    }
                    writer.WriteWhitespace("\n");
                }
                WriteLn(writer, "desc", el, prefix);
                WriteLn(writer, "donor", el, prefix, "Donated by ");
                writer.WriteElementString(prefix + "value", "Value: " + el.SelectSingleNode("value/text()").Value);
                writer.WriteElementString(prefix + "id", "\tAuction ID #: " + el.SelectSingleNode("id/text()").Value);
                writer.WriteWhitespace("\n");
            }

            writer.WriteEndElement();
            writer.Close();
        }

        private void WriteLn(XmlWriter writer, string name, XmlElement el, string prefix)
        {
            WriteLn(writer, name, el, prefix, "");
        }
        private void WriteLn(XmlWriter writer, string name, XmlElement el, string prefix, string valuePrefix)
        {
            writer.WriteElementString(prefix + name, valuePrefix + el.SelectSingleNode(name).InnerText);
            writer.WriteWhitespace("\n");
        }
    }

    public class LiveAuctionSortRule : PatchRule
    {
        public override void Patch(XmlElement directive, ref XmlDocument doc)
        {
            string[] ids = directive.InnerText.Split(',');
            Array.Reverse(ids);
            foreach (string strId in ids)
            {
                int id = int.Parse(strId);
                XmlNode node = doc.SelectSingleNode("/Root/item[id/text() = '" + id + "']");
                doc.DocumentElement.InsertAfter(
                    doc.DocumentElement.RemoveChild(node),
                    null);
            }
        }
    }
}
