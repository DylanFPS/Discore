import 'package:angular2/security.dart';

class WikiPage {
  static DomSanitizationService _dSS;
  SafeHtml body;
  SafeHtml sidebar;
  SafeHtml footer;

  WikiPage(String body) {
    this.body = _dSS.bypassSecurityTrustHtml(body);
    //this.sidebar = _dSS.bypassSecurityTrustHtml(sideBar);
    //this.footer = _dSS.bypassSecurityTrustHtml(footer);
  }

  static void setSanitizationService(DomSanitizationService dss) {
    _dSS = dss;
  }
}