import 'package:angular2/angular2.dart';
import 'dart:async';
import 'json_page_loader.dart';
import 'wiki_page.dart';

@Injectable()
class WikiService {
  Future<Map<String, WikiPage>> getPages() async
    => await JsonPageLoader.loadJson();
}