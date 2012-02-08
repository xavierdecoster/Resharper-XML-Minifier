using System;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Xml.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Resharper.Plugins.Minify
{
    [ContextAction(Name = "Resharper.Plugins.Xml.XmlMinifyElementContextAction", Group = "XML",
        Description = "Minifies the selected XML element by removing unnecessary spaces, tabs and carriage returns.", Priority = 0)]
    public class XmlMinifyElementContextAction : BulbItemImpl, IContextAction
    {
        protected IMinifier Minifier { get; private set; }
        protected XmlContextActionDataProvider DataProvider { get; private set; }

        public XmlMinifyElementContextAction(XmlContextActionDataProvider provider)
        {
            DataProvider = provider;
            Minifier = new XmlMinifier();
        }

        public override string Text
        {
            get
            {
                IXmlTag tag = DataProvider.FindNodeAtCaret<IXmlTag>();
                if (tag != null)
                {
                    if (tag.PathToRoot().IsNullOrEmpty())
                        return "Minify file";

                    return string.Format("Minify element '{0}'", tag.GetName());
                }

                return "Minify element";
            }
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            ITreeNode selectedElement = DataProvider.SelectedElement;
            if (selectedElement != null)
            {
                DocumentRange elementRange = selectedElement.GetDocumentRange();
                IPsiSourceFile sourceFile = selectedElement.GetSourceFile();
                if (sourceFile != null)
                {
                    IDocument document = sourceFile.Document;
                    string minifiedDocument = Minifier.Minify(elementRange.GetText());
                    document.ReplaceText(selectedElement.GetNavigationRange().TextRange, minifiedDocument);
                }
            }

            return null;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            IXmlTag tag = DataProvider.FindNodeAtCaret<IXmlTag>();

            if (tag != null && !tag.IsEmptyTag)
            {
                return true;
            }

            return false;
        }
    }
}
