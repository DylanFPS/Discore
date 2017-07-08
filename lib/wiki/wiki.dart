import 'dart:convert';
import 'dart:html'
  show CustomEvent, document;

import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';
import 'package:angular2/router.dart';

import 'wiki_page.dart';
import 'wiki_service.dart';

@Component(
  selector: 'discore-wiki',
  templateUrl: 'wiki.html',
  styleUrls: const ['wiki.css'],
  directives: const [COMMON_DIRECTIVES, SafeInnerHtmlDirective],
  providers: const [WikiService]
)
class WikiComponent implements OnInit {
  WikiPage currentPage;
  Map<String, WikiPage> rawPages;
  List<WikiPage> rawPagesList = new List();

  String title;

  final WikiService _service;
  final RouteParams _routeParams;

  WikiComponent(this._service, DomSanitizationService dss, this._routeParams) {
    WikiPage.setSanitizationService(dss);
  }

  ngOnInit() async {
    var pages = await _service.getPages();

    rawPages = new Map<String, WikiPage>()
      ..addAll(pages);

    String page = _routeParams.get('page');
    if (page != '' && rawPages.containsKey(page)) { changePage(page); }
    else { changePage("Home"); }
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