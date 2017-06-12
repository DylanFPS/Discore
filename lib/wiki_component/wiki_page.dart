import 'package:angular2/security.dart';

class WikiPage {
  static DomSanitizationService _dSS;
  SafeHtml body;

  WikiPage(String body) {
    this.body = _dSS.bypassSecurityTrustHtml(body);
  }

  static void setSanitizationService(DomSanitizationService dss) {
    _dSS = dss;
  }
}