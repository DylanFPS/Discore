import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';
import 'package:angular2/router.dart';

import 'wiki_page.dart';
import 'wiki_service.dart';

import 'dart:convert';
import 'dart:html'
  show CustomEvent, document;

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

  String title;

  final RouteParams _routeParams;

  WikiComponent(this._service, DomSanitizationService dss, this._routeParams) {
    WikiPage.setSanitizationService(dss);
  }

  void ngOnInit() {
    _service.getPages()
      .then((pages) {
          rawPages = new Map<String, WikiPage>()
            ..addAll(pages);

          String page = _routeParams.get('page');
          if (page != '' && rawPages.containsKey(page)) { changePage(page); }
          else { changePage("Home"); }
      });
  }

  void changePage(String page) {
    title = page.replaceAll(new RegExp(r'-'), ' ');
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

  SafeHtml getSidebar() {
    return rawPages['_Sidebar'].body;
  }
}