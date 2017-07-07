import 'package:angular2/angular2.dart';
import 'package:angular2/router.dart';


import "package:angular2/platform/common.dart"
    show LocationStrategy, HashLocationStrategy;

import 'wiki_component/wiki_component.dart';
import 'footer_component/footer_component.dart';
import 'index_component.dart';

@Component(
  selector: 'app',
  templateUrl: 'app_component.html',
  styleUrls: const ['app_component.css'],
  directives: const [ROUTER_DIRECTIVES, FooterComponent],
  providers: const[
    ROUTER_PROVIDERS,
    const Provider(LocationStrategy, useClass: HashLocationStrategy)
  ])
@RouteConfig(const [
  const Route(path: '/', name: 'Index', component: IndexComponent, useAsDefault: true),
  const Route(path: '/wiki', name: 'Wiki', component: WikiComponent),
  const Route(path: '/wiki/:page', name: 'WikiPage', component: WikiComponent)
])
class AppComponent {}