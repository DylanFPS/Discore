import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';
import 'wiki_page.dart';
import 'wiki_service.dart';
import 'dart:convert';
import 'dart:html';

@Component(
  selector: 'wiki',
  templateUrl: 'wiki.html',
  styleUrls: const ['wiki.css'],
  directives: const [COMMON_DIRECTIVES, SafeInnerHtmlDirective],
  providers: const [WikiService]
)
class WikiComponent implements OnInit {
  final WikiService _service;
  WikiPage currentPage;
  Map<String, WikiPage> rawPages;
  List<WikiPage> rawPagesList = new List();

  WikiComponent(this._service, DomSanitizationService dss) {
    WikiPage.setSanitizationService(dss);
  }

  void ngOnInit() {
    _service.getPages()
      .then(_setupPages);
  }

  void _setupPages(Map<String, WikiPage> pages) {
        rawPages = new Map<String, WikiPage>()
          ..addAll(pages);

      changePage("Home.md");
  }

  void changePage(String page) {
    currentPage = rawPages[page];
  }

  ///Load highlight.js
  ///Sweetfuck this is hacky, gets called at the end of this component's template
  void loadHL() {
    CustomEvent evnt = new CustomEvent('dartLoadHL');
    document.dispatchEvent(evnt);
  }

  void createTriangle(String elementID) {
    var payload = {"elementID": elementID};
    document.dispatchEvent(
      new CustomEvent('dartTrianglify', detail: JSON.encode(payload)));
  }

  SafeHtml getFooter() {
    return rawPages['_Footer.md'].body;
  }

  SafeHtml getSidebar() {
    return rawPages['_Sidebar.md'].body;
  }
}