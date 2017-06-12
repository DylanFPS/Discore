import 'package:angular2/angular2.dart';
import 'dart:html';
import 'dart:convert';

@Component(
  selector: "app-footer",
  templateUrl: 'footer.html',
  styleUrls: const ['footer.css'],
  directives: const [COMMON_DIRECTIVES],
)
class FooterComponent {
  void createTriangle(String elementID) {
    var payload = {"elementID": elementID};
    document.dispatchEvent(
      new CustomEvent('dartTrianglify', detail: JSON.encode(payload)));
  }
}