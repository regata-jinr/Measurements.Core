/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2020, REGATA Experiment at FLNP|JINR                       *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;

namespace Regata.Measurements.Managers
{
  public static partial class AppManager
  {
    public static event Action AppGoToLocalMode;
    public static event Action AppLeaveLocalMode;

    private static bool _localMode;
    public static bool LocalMode
    {
      get
      {
        return _localMode;
      }
      private set
      {
        _localMode = value;
        if (value)
          AppGoToLocalMode?.Invoke();
        else
          AppLeaveLocalMode?.Invoke();
      }
    }


    public static bool CheckLocalStorage()
    {
      throw new NotImplementedException();
    }

  } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
