import akka.actor.ActorSystem
import akka.http.scaladsl.Http
import akka.stream.{ActorMaterializer, Materializer}
import korolev._
import korolev.server._
import korolev.akkahttp._
import korolev.execution._
import korolev.state.javaSerialization._
import pravda.vm.Data.Primitive._

import scala.concurrent.Future

object AuctionApp extends App {

  import GuiState._
  import ApiHelper._
  import GuiState.globalContext._
  import GuiState.globalContext.symbolDsl._

  private implicit val actorSystem: ActorSystem = ActorSystem()
  private implicit val materializer: ActorMaterializer = ActorMaterializer()
  private val api:API = new API()

  private val config = KorolevServiceConfig[Future, GuiState, Any] (
    head = Seq(
        'link ('href /= "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.1/css/bulma.min.css", 'rel /= "stylesheet"),
        'link (
          'href        /= "https://use.fontawesome.com/releases/v5.0.13/css/all.css",
          'rel         /= "stylesheet",
          'crossorigin /= "anonymous",
          'integrity   /= "sha384-DNOHZ68U8hZfKXOrtjWvjxusGo9WQnrNx2sqG0tfsghAvtVlRW3tvkXWZh58N9jp"
        )
        //'link ('href /= "main.css", 'rel /= "stylesheet")
      ),
    stateStorage = StateStorage.default(MyItems()),
    render = {
      case state: MyItems =>
        mainLayout(state,   
        'div(
          'class /= "notification",
          'height @= "100%",
          'h3(
            'class /= "title is-3",
            "Мои предметы"
          ),
          'div(
            'class /= "columns",
            'div(
              'class /= "column is-8",
              'div(
                'class /= "tile is-ancestor",
                {
                  val total = GameItem.myItems.length
                  (0 to 4) map {
                    x =>
                      'div(
                        'class /= "tile is-vertical",
                        (0 to 3) map {
                          y =>
                            {
                              val index = y * 5 + x
                              'div(
                                'class /= "tile is-parent",
                                'div(
                                  'class /= "tile is-child",
                                  'div(
                                    'figure(
                                      'class /= "image is-128x128",
                                      'backgroundColor @= "#cacaca",
                                      'boxShadow @= (if (index == state.selectedItem) {"0 0 0 3px rgba(0,0,0,0.5)"} else {"none"}),
                                      if (index < total) {
                                        'div(
                                          event('click) { access => 
                                            access.transition(_ => state.copy(selectedItem = index))
                                          },
                                          'img(
                                            'src /= {
                                              val item = GameItem.myItems(index)
                                              GameItem.getItem(item).previewImage
                                            }
                                          )
                                        )
                                      } else {'div()}
                                    ),
                                    'span(
                                      'position @= "relative",
                                      'top @= "-1.5rem",
                                      //x + ", " + y + ": " + ((y - 1) * 5 + x - 1)
                                    )
                                  )
                                )
                              )
                            }
                        }
                      )
                  }
                }
              ),
              'br(),
              paginationMyLots(state)
            ),
            'div(
              'class /= "column is-4",
              'div(
                'class /= "notification",
                'backgroundColor @= "#e0e0e0",
                'height @= "100%",
                'width @= "100%",
                if(state.selectedItem < GameItem.myItems.length) {
                  val itemIndex = GameItem.myItems(state.selectedItem)
                  val item = GameItem.getItem(itemIndex)
                  'div(
                    'display @= "flex",
                    'flexFlow @= "column",
                    'height @= "100%",
                    'figure(
                      'class /= "image is-4by3",
                      'img(
                        'src /= item.bigImage
                      )
                    ),
                    'br(),
                    'span(
                      'strong(
                        item.name
                      )
                    ),
                    'span(
                      'strong(
                        "100 GameTokens"
                      )
                    ),
                    'div(
                      'flexGrow @= "1",
                      'position @= "relative",
                      'a(
                        'position @= "absolute",
                        'bottom @= "0",
                        'class /= "button is-warning",
                        'span(
                          "Продать"
                        ),
                        event('click) { access =>
                          access.transition(_ => state.copy(sellingItem = Some(item)))
                        }
                      )
                    )
                  )
                } else{
                  'div()
                }
              )
            )
          ),
        ),
        modal(
          isActive = !(state.sellingItem eq None),
          content = state.sellingItem match {
            case Some(item) =>
              'div(
                'span(
                  'strong(
                    item.name
                  )
                ),
                'br(),
                'a(
                  'class /= "button is-warning",
                  'span(
                    "Продать"
                  ),
                  /*event('click) { access =>
                    access.transition(_ => MyItems())
                  }*/
                )
              )
            case None => 'div()
          },
          onClose =
            event('click) { access =>
              access.transition(_ => state.copy(sellingItem = None))
            }
        )
      )
    case state: MyLots =>
      mainLayout(state,
          'div(
            'class /= "notification",
            'height @= "100%",
            'h3(
              'class /= "title is-3",
              "Мои лоты"
            ),
            'p(
              'class /= "control has-icons-left",
              'input(
                'class /= "input is-small",
                'type /= "text",
                'placeholder /= "search"
              ),
              'span(
                'class /= "icon is-small is-left",
                'i(
                  'class /= "fas fa-search",
                  'ariaHidden /= "true"
                )
              )
            ),
            'br(),
            'div(
              'class /= "columns",
              'div(
                'class /= "column is-9",
                'div(
                  'class /= "",
                  'div(
                    GameItem.myLots map {
                      x => 
                        myLotsItem(
                          GameItem.getItem(x),
                          event('click) { access =>
                            access.transition(_ => MyLots(Some(GameItem.getItem(x))))
                          }
                        )
                    }
                  )
                ),
                'br(),
                paginationMyLots(state)
              ),
              'div(
                'class /= "column is-3",
                'div(
                  'class /= "notification",
                  'backgroundColor @= "#e0e0e0",
                  'height @= "100%",
                  "Filter"
                )
              )
            )
          ),
          modal(
            isActive = !(state.removingItem eq None),
            content =
              'div(
                'span(
                  'strong(
                    "Вы уверены, что хотите убрать лот с продажи?"
                  )
                ),
                'br(),
                'div(
                  'class /= "buttons",
                  'a(
                    'class /= "button is-danger",
                    'span(
                      "Удалить"
                    ),
                    event('click) { access =>
                        access.transition(_ => MyItems())
                    }
                  ),
                  'a(
                    'class /= "button",
                    'span(
                      "Отмена"
                    ),
                    event('click) { access =>
                      access.transition(_ => MyLots())
                    }
                  )
                )
              ),
            onClose =
              event('click) { access =>
                access.transition(_ => MyLots())
              }
          )
        )
      case state: Auction =>
        mainLayout(state,
            'div(
              'class /= "notification",
              'height @= "100%",
              'h3(
                'class /= "title is-3",
                "Торговая площадка"
              ),
              'div(
                'class /= "",
                'p(
                  'class /= "control has-icons-left",
                  'input(
                    'class /= "input is-small",
                    'type /= "text",
                    'placeholder /= "search"
                  ),
                  'span(
                    'class /= "icon is-small is-left",
                    'i(
                      'class /= "fas fa-search",
                      'ariaHidden /= "true"
                    )
                  )
                )
              ),
              'br(),
              'div(
                'class /= "columns",
                'div(
                  'class /= "column is-9",
                  'div(
                    'class /= "",
                    'div(
                      GameItem.auction map {
                        x => 
                          auctionItem(
                            GameItem.getItem(x),
                            event('click) { access =>
                              access.transition(_ => Auction(Some(GameItem.getItem(x))))
                            }
                          )
                      }
                    )
                  ),
                  'br(),
                  paginationMyLots(state)
                ),
                'div(
                  'class /= "column is-3",
                  'div(
                    'class /= "notification",
                    'backgroundColor @= "#e0e0e0",
                    'height @= "100%",
                    "Filter"
                  )
                )
              )
            ),
            modal(
              isActive = !(state.buyingItem eq None),
              content =
                'div(
                  'span(
                    'strong(
                      state.buyingItem match {
                        case Some(item) => item.name
                        case None => ""
                      }
                    )
                  ),
                  if (state.result != None) 'div(state.result) else void,
                  'br(),
                  'a(
                    'class /= ("button is-success" + (if(state.inProgress) " is-loading" else "")),
                    if(state.inProgress) 'disabled /= "" else void,
                    'span(
                      "Купить"
                    ),
                    event('click) { access =>
                      for {
                        _ <- access.transition(_ => state.copy(inProgress = true))
                        result <- api.test(Utf8("world"))
                        _ <- access.transition {
                          result match {
                            case Left(error) =>
                              _ => state.copy(inProgress = false, result = Some(error))
                            case Right(meta) =>
                              _ => state.copy(inProgress = false, result = Some(meta.data))
                          }
                        }
                      } yield {
                        ()
                      }
                    }
                  )
                ),
              onClose =
                event('click) { access =>
                  access.transition(_ => Auction())
                }
            )
          )
    },
    router = emptyRouter
  )


  private def mainLayout(state: GuiState, content: Node*) = {
    'body(
      'nav(
        'class /= "navbar",
        'role /= "navigation",
        'ariaLabel /= "main navigation",
        'div(
          'class /= "navbar-brand",
          'a(
            'class /= "navbar-item",
            'p(
              'fontWeight @= "600",
              'fontSize @= "2 rem",
              "EXPLOAD"
            )
          ),
          'a(
            'role /= "button",
            'class /= "navbar-burger burger",
            'ariaLabel /= "menu",
            'ariaExpanded /= "false",
            'dataTarget /= "navbarBasicExample",
            'span(
              'ariaHidden /= "true"
            ),
            'span(
              'ariaHidden /= "true"
            ),
            'span(
              'ariaHidden /= "true"
            )
          )
        ),
        'div(
          'id /= "navbar",
          'class /= "navbar-menu",
          'div(
            'class /= "navbar-start",
            'a(
              'class /= (state match {
                case state: MyItems =>
                  "navbar-item is-active"
                case _ =>
                  "navbar-item"
              }),
              "Мои предметы",
              event('click) { access =>
                access.transition(_ => MyItems())
              }
            ),
            'a(
              'class /= (state match {
                case state: MyLots =>
                  "navbar-item is-active"
                case _ =>
                  "navbar-item"
              }),
              "Мои лоты",
              event('click) { access =>
                access.transition(_ => MyLots())
              }
            ),
            'a(
              'class /= (state match {
                case state: Auction =>
                  "navbar-item is-active"
                case _ =>
                  "navbar-item"
              }),
              "Аукцион",
              event('click) { access =>
                access.transition(_ => Auction())
              }
            )
          ),
          'div(
            'class /= "navbar-end",
            'div(
              'class /= "navbar-item",
              'div(
                'marginRight @= "1rem",
                'span(
                  'strong(
                    "1,234,567 GameTokens"
                  )
                )
              ),
              'div(
                'class /= "buttons",
                'a(
                  'class /= "button is-primary",
                  'strong(
                    "Sign up"
                  )
                ),
                'a(
                  'class /= "button is-light",
                  "Log in"
                )
              )
            )
          )
        )
      ),
      'section(
        'class /= "section",
        'div(
          'class /= "container",
          content
        )
      )
    )
  }

  private def modal(isActive: Boolean, content: Node, onClose: Node) = {
    'div(
      'class /= "modal" + (if (isActive) {" is-active"} else {""}),
      'div(
        'class /= "modal-background",
        'backgroundColor @= "rgba(0,0,0,0)",
        onClose
      ),
      'div(
        'class /= "modal-content",
        'div(
          'class /= "box",
          'article(
            'class /= "media",
            'div(
              'class /= "media-content",
              'div(
                'class /= "content",
                content
              )
            ),
            'div(
              'class /= "media-right",
              'button(
                'class /= "delete",
                onClose
              )
            )
          )
        )
      )
    )
  }

  private def myLotsItem(item: GameItem, onRemove: Node) = {
    'div(
      'class /= "notification",
      'backgroundColor @= "#e0e0e0",
      'article(
        'class /= "media",
        'figure(
          'class /= "media-left",
          'p(
            'class /= "image is-128x128",
            'img(
              'src /= item.previewImage
            )
          )
        ),
        'div(
          'class /= "media-content",
          'div(
            'class /= "content",
            'p(
              'strong(
                item.name
              )
            )
          )
        ),
        'div(
          'class /= "media-right",
          'a(
            'class /= "button is-danger",
            'span(
              "Удалить"
            ),
            onRemove
          )
        )
      )
    )
  }

  private def auctionItem(item: GameItem, onBuy: Node) = {
    'div(
      'class /= "notification",
      'backgroundColor @= "#e0e0e0",
      'article(
        'class /= "media",
        'figure(
          'class /= "media-left",
          'p(
            'class /= "image is-128x128",
            'img(
              'src /= item.previewImage
            )
          )
        ),
        'div(
          'class /= "media-content",
          'div(
            'class /= "content",
            'p(
              'strong(
                item.name
              )
            )
          )
        ),
        'div(
          'class /= "media-right",
          'a(
            'class /= "button is-success",
            'span(
              "Купить"
            ),
            onBuy
          )
        )
      )
    )
  }

  private def paginationMyLots(state: GuiState) = {
    'nav(
      'class /= "pagination is-centered",
      'role /= "navigation",
      'ariaLabel /= "pagination",
      'a(
        'class /= "pagination-previous",
        "Previous"
      ),
      'a(
        'class /= "pagination-next",
        "Next page"
      ),
      'ul(
        'class /= "pagination-list",
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 1",
            "1"
          )
        ),
        'li(
          'span(
            'class /= "pagination-ellipsis",
            "…"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 45",
            "45"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link is-current",
            'ariaLabel /= "Page 46",
            'ariaCurrent /= "page",
            "46"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 47",
            "47"
          )
        ),
        'li(
          'span(
            'class /= "pagination-ellipsis",
            "…"
          )
        ),
        'li(
          'a(
            'class /= "pagination-link",
            'ariaLabel /= "Goto page 86",
            "86"
          )
        )
      )
    )
  }

  private val route = akkaHttpService(config).apply(AkkaHttpServerConfig())

  Http().bindAndHandle(route, "0.0.0.0", 8080)
}
