import 'package:angular2/angular2.dart';
import 'package:angular2/router.dart';
import "package:angular2/platform/common.dart"
    show LocationStrategy, HashLocationStrategy;

import 'footer/footer.dart';

import 'index/index_component.dart';
import 'wiki/wiki.dart';

@Component(
  selector: 'discore-app',
  templateUrl: 'app_component.html',
  styleUrls: const ['app_component.css'],
  directives: const [
    ROUTER_DIRECTIVES, 
    FooterComponent
  ],
  providers: const[
    ROUTER_PROVIDERS,
    const Provider(LocationStrategy, useClass: HashLocationStrategy)
  ])
@RouteConfig(const [
  const Route(path: '/', name: 'Index', component: IndexComponent, useAsDefault: true),
  const Route(path: '/wiki/2x', name: 'Wiki', component: WikiComponent),
  const Route(path: '/wiki/2x/:page', name: 'WikiPage', component: WikiComponent),

  const Redirect(path: '/**', redirectTo: const ['Index'])
])
class AppComponent { }